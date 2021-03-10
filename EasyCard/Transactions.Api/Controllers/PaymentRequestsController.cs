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
                    IOptions<ApiSettings> apiSettings)
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
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(PaymentRequestSummary)
                    .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Select(d => d.GetColMeta(InvoiceSummaryResource.ResourceManager, System.Globalization.CultureInfo.InvariantCulture))
                    .ToDictionary(d => d.Key)
            };
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<PaymentRequestSummary>>> GetPaymentRequests([FromQuery] PaymentRequestsFilter filter)
        {
            var query = paymentRequestsService.GetPaymentRequests().Filter(filter);
            var numberOfRecordsFuture = query.DeferredCount().FutureValue();

            using (var dbTransaction = paymentRequestsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<PaymentRequestSummary>();

                response.Data = await mapper.ProjectTo<PaymentRequestSummary>(query.OrderByDynamic(filter.SortBy ?? nameof(PaymentRequest.PaymentRequestTimestamp), filter.SortDesc).ApplyPagination(filter)).ToListAsync();
                response.NumberOfRecords = numberOfRecordsFuture.Value;
                return Ok(response);
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

                paymentRequest.PaymentRequestUrl = GetPaymentRequestUrl(dbPaymentRequest, terminal.SharedApiKey)?.Item1;

                paymentRequest.History = await mapper.ProjectTo<PaymentRequestHistorySummary>(paymentRequestsService.GetPaymentRequestHistory(dbPaymentRequest.PaymentRequestID).OrderByDescending(d => d.PaymentRequestHistoryID)).ToListAsync();

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
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            // Check consumer
            var consumer = model.DealDetails.ConsumerID != null ? EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == model.DealDetails.ConsumerID && d.TerminalID == terminal.TerminalID), "Consumer") : null;

            var newPaymentRequest = mapper.Map<PaymentRequest>(model);

            // Update details if needed
            newPaymentRequest.DealDetails.UpdateDealDetails(consumer, terminal.Settings, newPaymentRequest);
            if (model.IssueInvoice.GetValueOrDefault())
            {
                model.InvoiceDetails.UpdateInvoiceDetails(terminal.InvoiceSettings);
            }

            if (consumer != null)
            {
                newPaymentRequest.CardOwnerName = consumer.ConsumerName;
                newPaymentRequest.CardOwnerNationalID = consumer.ConsumerNationalID;
            }

            newPaymentRequest.Calculate();

            newPaymentRequest.MerchantID = terminal.MerchantID;

            newPaymentRequest.ApplyAuditInfo(httpContextAccessor);

            await paymentRequestsService.CreateEntity(newPaymentRequest);

            var response = CreatedAtAction(nameof(GetPaymentRequest), new { paymentRequestID = newPaymentRequest.PaymentRequestID }, new OperationResponse(Transactions.Shared.Messages.PaymentRequestCreated, StatusEnum.Success, newPaymentRequest.PaymentRequestID));

            if (terminal.SharedApiKey == null)
            {
                return BadRequest(new OperationResponse("Please add Shared Api Key first", StatusEnum.Error, newPaymentRequest.PaymentRequestID, httpContextAccessor.TraceIdentifier));
            }

            await emailSender.SendEmail(BuildPaymentRequestEmail(newPaymentRequest, terminal));

            await paymentRequestsService.UpdateEntityWithStatus(newPaymentRequest, Shared.Enums.PaymentRequestStatusEnum.Sent);

            return response;
        }

        private Tuple<string, string> GetPaymentRequestUrl(PaymentRequest dbPaymentRequest, byte[] sharedTerminalApiKey)
        {
            if (sharedTerminalApiKey == null)
            {
                return null;
            }

            var uriBuilder = new UriBuilder(apiSettings.CheckoutPortalUrl);
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["paymentRequest"] = Convert.ToBase64String(dbPaymentRequest.PaymentRequestID.ToByteArray());
            query["apiKey"] = Convert.ToBase64String(sharedTerminalApiKey);

            if (dbPaymentRequest.DealDetails?.ConsumerID.HasValue == true)
            {
                query["consumerID"] = dbPaymentRequest.DealDetails.ConsumerID.Value.ToString();
            }

            uriBuilder.Query = query.ToString();
            var url = uriBuilder.ToString();

            query["reject"] = true.ToString();
            uriBuilder.Query = query.ToString();
            var rejectUrl = uriBuilder.ToString();

            return Tuple.Create(url, rejectUrl);
        }

        private Email BuildPaymentRequestEmail(PaymentRequest paymentRequest, Terminal terminal)
        {
            var settings = terminal.PaymentRequestSettings;

            var emailSubject = paymentRequest.RequestSubject ?? settings.DefaultRequestSubject;
            var emailTemplateCode = settings.EmailTemplateCode ?? nameof(PaymentRequest);
            var url = GetPaymentRequestUrl(paymentRequest, terminal.SharedApiKey);
            var substitutions = new List<TextSubstitution>();

            substitutions.Add(new TextSubstitution(nameof(settings.MerchantLogo), string.IsNullOrWhiteSpace(settings.MerchantLogo) ? $"{apiSettings.CheckoutPortalUrl}/img/merchant-logo.png" : settings.MerchantLogo));
            substitutions.Add(new TextSubstitution("PayWithEasyCardBtnUrl", $"{apiSettings.CheckoutPortalUrl}/img/pay-with-easycard.png")); // TODO: make dynamic path to fill "viewed" status
            substitutions.Add(new TextSubstitution(nameof(paymentRequest.DueDate), paymentRequest.DueDate.GetValueOrDefault().ToString("d"))); // TODO: locale
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
    }
}