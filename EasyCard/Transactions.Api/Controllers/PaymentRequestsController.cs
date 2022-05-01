using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Business.Security;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Transactions.Api.Extensions.Filtering;
using Transactions.Api.Models.Invoicing;
using Shared.Api.Extensions;
using Shared.Helpers.Security;
using Transactions.Api.Models.PaymentRequests;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Transactions.Shared;
using Microsoft.Extensions.Options;
using Transactions.Api.Extensions;
using Shared.Helpers.Email;
using Merchants.Business.Entities.Terminal;
using Shared.Helpers.Templating;
using Shared.Helpers;
using Z.EntityFramework.Plus;
using Shared.Api.Configuration;
using Transactions.Shared.Enums;
using Shared.Integration;
using Shared.Helpers.Sms;
using Merchants.Business.Extensions;
using System.Web;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Controllers
{
    [Route("api/paymentRequests")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
    [ApiController]
    public class PaymentRequestsController : ApiControllerBase
    {
        private readonly IPaymentRequestsService paymentRequestsService;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly IConsumersService consumersService;
        private readonly ITerminalsService terminalsService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ApplicationSettings appSettings;
        private readonly ApiSettings apiSettings;
        private readonly ICryptoServiceCompact cryptoServiceCompact;
        private readonly ISystemSettingsService systemSettingsService;
        private readonly IEmailSender emailSender;
        private readonly ISmsService smsService;
        private readonly ITransactionsService transactionsService;

        public PaymentRequestsController(
                    IPaymentRequestsService paymentRequestsService,
                    IMapper mapper,
                    ITerminalsService terminalsService,
                    ILogger<CardTokenController> logger,
                    IHttpContextAccessorWrapper httpContextAccessor,
                    IConsumersService consumersService,
                    IOptions<ApplicationSettings> appSettings,
                    ICryptoServiceCompact cryptoServiceCompact,
                    ISystemSettingsService systemSettingsService,
                    IEmailSender emailSender,
                    IOptions<ApiSettings> apiSettings,
                    ISmsService smsService,
                    ITransactionsService transactionsService)
        {
            this.paymentRequestsService = paymentRequestsService;
            this.mapper = mapper;
            this.apiSettings = apiSettings.Value;
            this.terminalsService = terminalsService;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.consumersService = consumersService;
            this.appSettings = appSettings.Value;
            this.cryptoServiceCompact = cryptoServiceCompact;
            this.systemSettingsService = systemSettingsService;
            this.emailSender = emailSender;
            this.smsService = smsService;
            this.transactionsService = transactionsService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = (httpContextAccessor.GetUser().IsAdmin() ? typeof(PaymentRequestSummaryAdmin) : typeof(PaymentRequestSummary))
                    .GetObjectMeta(PaymentRequestSummaryResource.ResourceManager, CurrentCulture)
            };
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<PaymentRequestSummary>>> GetPaymentRequests([FromQuery] PaymentRequestsFilter filter)
        {
            var query = paymentRequestsService.GetPaymentRequests().Filter(filter);
            var numberOfRecordsFuture = query.DeferredCount().FutureValue();

            using (var dbTransaction = paymentRequestsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                if (httpContextAccessor.GetUser().IsAdmin())
                {
                    var response = new SummariesResponse<PaymentRequestSummaryAdmin>();

                    var summary = await mapper.ProjectTo<PaymentRequestSummaryAdmin>(query.OrderByDynamic(filter.SortBy ?? nameof(PaymentRequest.PaymentRequestTimestamp), filter.SortDesc)
                        .ApplyPagination(filter)).ToListAsync();

                    var terminalsId = summary.Select(t => t.TerminalID).Distinct();

                    var terminals = await terminalsService.GetTerminals()
                        .Include(t => t.Merchant)
                        .Where(t => terminalsId.Contains(t.TerminalID))
                        .Select(t => new { t.TerminalID, t.Label, t.Merchant.BusinessName })
                        .ToDictionaryAsync(k => k.TerminalID, v => new { v.Label, v.BusinessName });

                    //TODO: Merchant name instead of BusinessName
                    summary.ForEach(s =>
                    {
                        if (s.TerminalID.HasValue && terminals.ContainsKey(s.TerminalID.Value))
                        {
                            s.TerminalName = terminals[s.TerminalID.Value].Label;
                            s.MerchantName = terminals[s.TerminalID.Value].BusinessName;
                        }
                    });

                    response.Data = summary;
                    response.NumberOfRecords = numberOfRecordsFuture.Value;
                    return Ok(response);
                }
                else
                {
                    var response = new SummariesResponse<PaymentRequestSummary>();

                    response.Data = await mapper.ProjectTo<PaymentRequestSummary>(query.OrderByDynamic(filter.SortBy ?? nameof(PaymentRequest.PaymentRequestTimestamp), filter.SortDesc)
                        .ApplyPagination(filter)).ToListAsync();
                    response.NumberOfRecords = numberOfRecordsFuture.Value;
                    return Ok(response);
                }
            }
        }

        [HttpGet]
        [Route("{paymentRequestID}")]
        public async Task<ActionResult<PaymentRequestResponse>> GetPaymentRequest([FromRoute] Guid paymentRequestID)
        {
            using (var dbTransaction = paymentRequestsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var dbPaymentRequest = EnsureExists(await paymentRequestsService.GetPaymentRequests().FirstOrDefaultAsync(m => m.PaymentRequestID == paymentRequestID));

                var terminal = EnsureExists(await terminalsService.GetTerminal(dbPaymentRequest.TerminalID.GetValueOrDefault()));

                var paymentRequest = mapper.Map<PaymentRequestResponse>(dbPaymentRequest);

                paymentRequest.History = await mapper.ProjectTo<PaymentRequestHistorySummary>(paymentRequestsService.GetPaymentRequestHistory(dbPaymentRequest.PaymentRequestID).OrderByDescending(d => d.PaymentRequestHistoryID)).ToListAsync();

                paymentRequest.TerminalName = terminal.Label;

                var transaction = await transactionsService.GetTransaction(t => t.PaymentRequestID == paymentRequestID);

                if (transaction != null)
                {
                    paymentRequest.UserPaidDetails = mapper.Map<PaymentRequestUserPaidDetails>(transaction);
                }

                return Ok(paymentRequest);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> CreatePaymentRequest([FromBody] PaymentRequestCreate model)
        {
            var merchantID = User.GetMerchantID();
            var userIsTerminal = User.IsTerminal();

            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID.Value));

            if (terminal.EnabledFeatures == null || !terminal.EnabledFeatures.Any(f => f == Merchants.Shared.Enums.FeatureEnum.Checkout))
            {
                return BadRequest(new OperationResponse(Messages.CheckoutFeatureMustBeEnabled, StatusEnum.Error));
            }

            // TODO: validation procedure
            //if (model.AllowPinPad == true && !(model.PaymentRequestAmount > 0))
            //{
            //    return BadRequest(new OperationResponse(Messages.AmountRequiredForPinpadDeal, StatusEnum.Error));
            //}

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            // Check consumer
            var consumer = model.DealDetails.ConsumerID != null ? EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == model.DealDetails.ConsumerID), "Consumer") : null;

            if (model.IssueInvoice.GetValueOrDefault())
            {
                if (model.InvoiceDetails == null)
                {
                    model.InvoiceDetails = new SharedIntegration.Models.Invoicing.InvoiceDetails { InvoiceType = terminal.InvoiceSettings.DefaultInvoiceType.GetValueOrDefault() };
                }

                model.InvoiceDetails.UpdateInvoiceDetails(terminal.InvoiceSettings);
            }

            if (model.AllowPinPad.GetValueOrDefault())
            {
                model.PinPadDetails = model.PinPadDetails.UpdatePinPadDetails(terminal.Integrations.FirstOrDefault(i => i.ExternalSystemID == ExternalSystemHelpers.NayaxPinpadProcessorExternalSystemID));
            }

            if (!model.VATRate.HasValue)
            {
                model.VATRate = terminal.Settings.VATRate;
            }

            var newPaymentRequest = mapper.Map<PaymentRequest>(model);

            newPaymentRequest.Calculate();

            // Update details if needed
            newPaymentRequest.DealDetails.UpdateDealDetails(consumer, terminal.Settings, newPaymentRequest, null);

            if (consumer != null)
            {
                newPaymentRequest.CardOwnerName = consumer.ConsumerName;
                newPaymentRequest.CardOwnerNationalID = consumer.ConsumerNationalID;
            }
            else
            {
                var consumerID = await CreateConsumer(model, merchantID.Value);
                if (consumerID.HasValue)
                {
                    newPaymentRequest.DealDetails.ConsumerID = consumerID;
                }
            }

            if (string.IsNullOrWhiteSpace(newPaymentRequest.DealDetails.ConsumerEmail))
            {
                // TODO: prper message
                return BadRequest(new OperationResponse("Email required", StatusEnum.Error, newPaymentRequest.PaymentRequestID, httpContextAccessor.TraceIdentifier));
            }

            newPaymentRequest.MerchantID = terminal.MerchantID;

            newPaymentRequest.ApplyAuditInfo(httpContextAccessor);

            await paymentRequestsService.CreateEntity(newPaymentRequest);

            var response = CreatedAtAction(nameof(GetPaymentRequest), new { paymentRequestID = newPaymentRequest.PaymentRequestID }, new OperationResponse(Transactions.Shared.Messages.PaymentRequestCreated, StatusEnum.Success, newPaymentRequest.PaymentRequestID));

            //if (terminal.SharedApiKey == null)
            //{
            //    return BadRequest(new OperationResponse("Please add Shared Api Key first", StatusEnum.Error, newPaymentRequest.PaymentRequestID, httpContextAccessor.TraceIdentifier));
            //}

            await emailSender.SendEmail(BuildPaymentRequestEmail(newPaymentRequest, terminal));

            if (newPaymentRequest.DealDetails.ConsumerPhone != null
                && terminal.FeatureEnabled(Merchants.Shared.Enums.FeatureEnum.SmsNotification))
            {
                await SendPaymentRequestSMS(newPaymentRequest, terminal);
            }

            newPaymentRequest.PaymentRequestUrl = GetPaymentRequestSMSUrl(newPaymentRequest);

            await paymentRequestsService.UpdateEntityWithStatus(newPaymentRequest, Shared.Enums.PaymentRequestStatusEnum.Sent);

            return response;
        }

        [HttpDelete]
        [Route("{paymentRequestID}")]
        public async Task<ActionResult<OperationResponse>> CancelPaymentRequest([FromRoute] Guid paymentRequestID)
        {
            var dbPaymentRequest = EnsureExists(await paymentRequestsService.GetPaymentRequests().FirstOrDefaultAsync(m => m.PaymentRequestID == paymentRequestID));

            if (dbPaymentRequest.Status == PaymentRequestStatusEnum.Payed || (int)dbPaymentRequest.Status < 0 || dbPaymentRequest.PaymentTransactionID != null)
            {
                return BadRequest(new OperationResponse($"{Messages.PaymentRequestStatusIsClosed}", StatusEnum.Error, dbPaymentRequest.PaymentRequestID, httpContextAccessor.TraceIdentifier));
            }

            await paymentRequestsService.UpdateEntityWithStatus(dbPaymentRequest, PaymentRequestStatusEnum.Canceled, message: Messages.PaymentRequestCanceled);

            return Ok(new OperationResponse { EntityUID = paymentRequestID, Status = StatusEnum.Success, Message = Messages.PaymentRequestCanceled });
        }

        private Tuple<string, string> GetPaymentRequestUrl(PaymentRequest dbPaymentRequest)
        {
            var uriBuilder = new UriBuilder(apiSettings.CheckoutPortalUrl);
            uriBuilder.Path = "/p";
            var encrypted = cryptoServiceCompact.EncryptCompact(dbPaymentRequest.PaymentRequestID.ToByteArray());

            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["r"] = encrypted;

            uriBuilder.Query = query.ToString();

            if (!string.IsNullOrWhiteSpace(dbPaymentRequest.Language))
            {
                query["l"] = dbPaymentRequest.Language;
            }

            var url = uriBuilder.ToString();

            query["reject"] = true.ToString();
            uriBuilder.Query = query.ToString();
            var rejectUrl = uriBuilder.ToString();

            return Tuple.Create(url, rejectUrl);
        }

        private string GetPaymentRequestSMSUrl(PaymentRequest dbPaymentRequest)
        {
            var uriBuilder = new UriBuilder(apiSettings.CheckoutPortalUrl);
            uriBuilder.Path = "/p";
            var encrypted = cryptoServiceCompact.EncryptCompact(dbPaymentRequest.PaymentRequestID.ToByteArray());

            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["r"] = encrypted;

            if (!string.IsNullOrWhiteSpace(dbPaymentRequest.Language))
            {
                query["l"] = dbPaymentRequest.Language;
            }

            uriBuilder.Query = query.ToString();

            return uriBuilder.ToString();
        }

        private Email BuildPaymentRequestEmail(PaymentRequest paymentRequest, Terminal terminal)
        {
            var settings = terminal.PaymentRequestSettings;

            var emailSubject = paymentRequest.RequestSubject ?? (paymentRequest.IsRefund ? settings.DefaultRefundRequestSubject : settings.DefaultRequestSubject);
            var emailTemplateCode = $"{settings.EmailTemplateCode ?? nameof(PaymentRequest)}{(paymentRequest.IsRefund ? "Refund" : string.Empty)}";
            var url = GetPaymentRequestUrl(paymentRequest);
            var substitutions = new List<TextSubstitution>();

            substitutions.Add(new TextSubstitution(nameof(settings.MerchantLogo), string.IsNullOrWhiteSpace(settings.MerchantLogo) ? $"{apiSettings.CheckoutPortalUrl}/img/merchant-logo.png" : $"{apiSettings.BlobBaseAddress}/{settings.MerchantLogo}"));
            substitutions.Add(new TextSubstitution("PayWithEasyCardBtnUrl", $"{apiSettings.CheckoutPortalUrl}/img/pay-with-easycard.png")); // TODO: make dynamic path to fill "viewed" status
            substitutions.Add(new TextSubstitution(nameof(paymentRequest.DueDate), paymentRequest.DueDate.HasValue ? paymentRequest.DueDate.Value.ToString("dd/MM/yyyy") : "-")); // TODO: locale
            substitutions.Add(new TextSubstitution(nameof(paymentRequest.PaymentRequestAmount), $"{paymentRequest.PaymentRequestAmount.ToString("F2")}{paymentRequest.Currency.GetCurrencySymbol()}"));
            substitutions.Add(new TextSubstitution("PaymentRequestUrl", url.Item1));
            substitutions.Add(new TextSubstitution("RejectPaymentRequestUrl", url.Item2));
            substitutions.Add(new TextSubstitution(nameof(terminal.Merchant.MarketingName), terminal.Merchant.MarketingName ?? terminal.Merchant.BusinessName));

            var email = new Email
            {
                EmailTo = paymentRequest.DealDetails?.ConsumerEmail,
                Subject = emailSubject,
                TemplateCode = emailTemplateCode,
                Substitutions = substitutions.ToArray()
            };

            return email;
        }

        private Task<OperationResponse> SendPaymentRequestSMS(PaymentRequest paymentRequest, Terminal terminal)
        {
            if (string.IsNullOrWhiteSpace(paymentRequest.DealDetails.ConsumerPhone))
            {
                return Task.FromResult(new OperationResponse { Status = StatusEnum.Error, Message = "DealDetails ConsumerPhone is null" });
            }

            var settings = terminal.PaymentRequestSettings;
            var messageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            var template = paymentRequest.IsRefund ? Resources.SMSResource.PaymentRequestRefund : Resources.SMSResource.PaymentRequest;
            var url = GetPaymentRequestSMSUrl(paymentRequest);

            //TODO: due date?
            template = template.Replace("{Merchant}", terminal.Merchant.MarketingName ?? terminal.Merchant.BusinessName)
                .Replace("{Customer}", paymentRequest.DealDetails.ConsumerName ?? paymentRequest.CardOwnerName)
                .Replace("{Amount}", $"{paymentRequest.PaymentRequestAmount.ToString("F2")}{paymentRequest.Currency.GetCurrencySymbol()}")
                .Replace("{PaymentLink}", url);

            return smsService.Send(new SmsMessage
            {
                MerchantID = terminal.MerchantID,
                MessageId = messageId,
                Body = template,
                From = string.IsNullOrWhiteSpace(settings.FromPhoneNumber) ? appSettings.SmsFrom : settings.FromPhoneNumber,
                To = paymentRequest.DealDetails.ConsumerPhone,
                CorrelationId = httpContextAccessor.GetCorrelationId()
            });
        }

        private async Task<Guid?> CreateConsumer(PaymentRequestCreate transaction, Guid merchantID)
        {
            try
            {
                var consumer = new Merchants.Business.Entities.Billing.Consumer();

                mapper.Map(transaction.DealDetails, consumer);
                consumer.ConsumerName = transaction.DealDetails?.ConsumerName;
                consumer.ConsumerEmail = transaction.DealDetails?.ConsumerEmail;
                consumer.ConsumerNationalID = transaction.CardOwnerNationalID;
                consumer.MerchantID = merchantID;
                consumer.ApplyAuditInfo(httpContextAccessor);

                if (!(!string.IsNullOrWhiteSpace(consumer.ConsumerName) && !string.IsNullOrWhiteSpace(consumer.ConsumerEmail)))
                {
                    return null;
                }

                await consumersService.CreateEntity(consumer);

                return consumer.ConsumerID;
            }
            catch (Exception ex)
            {
                logger.LogError($"Cannot create consumer: {ex.Message}", ex);
                return null;
            }
        }
    }
}