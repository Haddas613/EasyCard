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
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Transactions.Shared;
using Shared.Helpers;
using Transactions.Api.Services;
using Microsoft.Extensions.Options;
using Shared.Helpers.Queue;
using Shared.Integration.Models.Invoicing;
using Shared.Helpers.Email;
using Merchants.Shared.Models;
using Shared.Helpers.Templating;
using Merchants.Business.Entities.Terminal;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Controllers
{
    [Route("api/invoicing")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
    [ApiController]
    public class InvoicingController : ApiControllerBase
    {
        private readonly IInvoiceService invoiceService;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly IConsumersService consumersService;
        private readonly ITerminalsService terminalsService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ISystemSettingsService systemSettingsService;
        private readonly IInvoicingResolver invoicingResolver;
        private readonly ApplicationSettings appSettings;
        private readonly IQueue queue;
        private readonly IEmailSender emailSender;

        public InvoicingController(
                    IInvoiceService invoiceService,
                    IMapper mapper,
                    ITerminalsService terminalsService,
                    ILogger<CardTokenController> logger,
                    IHttpContextAccessorWrapper httpContextAccessor,
                    IConsumersService consumersService,
                    ISystemSettingsService systemSettingsService,
                    IInvoicingResolver invoicingResolver,
                    IOptions<ApplicationSettings> appSettings,
                    IQueueResolver queueResolver,
                    IEmailSender emailSender)
        {
            this.invoiceService = invoiceService;
            this.mapper = mapper;

            this.terminalsService = terminalsService;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.consumersService = consumersService;
            this.systemSettingsService = systemSettingsService;
            this.appSettings = appSettings.Value;
            this.queue = queueResolver.GetQueue(QueueResolver.InvoiceQueue);
            this.emailSender = emailSender;
            this.invoicingResolver = invoicingResolver;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(InvoiceSummary)
                    .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Select(d => d.GetColMeta(InvoiceSummaryResource.ResourceManager, System.Globalization.CultureInfo.InvariantCulture))
                    .ToDictionary(d => d.Key)
            };
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<InvoiceSummary>>> GetInvoices([FromQuery] InvoicesFilter filter)
        {
            var query = invoiceService.GetInvoices().Filter(filter);

            using (var dbTransaction = invoiceService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<InvoiceSummary> { NumberOfRecords = await query.CountAsync() };

                response.Data = await mapper.ProjectTo<InvoiceSummary>(query.OrderByDescending(i => i.InvoiceID).ApplyPagination(filter)).ToListAsync();

                return Ok(response);
            }
        }

        [HttpGet]
        [Route("{invoiceID}")]
        public async Task<ActionResult<InvoiceResponse>> GetInvoice([FromRoute] Guid invoiceID)
        {
            using (var dbTransaction = invoiceService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var dbInvoice = EnsureExists(await invoiceService.GetInvoices().FirstOrDefaultAsync(m => m.InvoiceID == invoiceID));

                var invoice = mapper.Map<InvoiceResponse>(dbInvoice);

                return Ok(invoice);
            }
        }

        // NOTE: this creates only db record - Invoicing system integration should be processed in a queue
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> CreateInvoice([FromBody] InvoiceRequest model)
        {
            var merchantID = User.GetMerchantID();
            var userIsTerminal = User.IsTerminal();

            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            var newInvoice = mapper.Map<Invoice>(model);

            newInvoice.MerchantID = terminal.MerchantID;

            newInvoice.ApplyAuditInfo(httpContextAccessor);

            if (newInvoice.DealDetails == null)
            {
                newInvoice.DealDetails = new Business.Entities.DealDetails();
            }

            if (newInvoice.InvoiceDetails == null)
            {
                newInvoice.InvoiceDetails = new SharedIntegration.Models.Invoicing.InvoiceDetails { InvoiceType = terminal.InvoiceSettings.DefaultInvoiceType.GetValueOrDefault() };
            }

            // Check consumer
            var consumer = newInvoice.DealDetails.ConsumerID != null ? EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == newInvoice.DealDetails.ConsumerID && d.TerminalID == terminal.TerminalID), "Consumer") : null;

            if (consumer != null)
            {
                if (!string.IsNullOrWhiteSpace(consumer.ConsumerNationalID) && !string.IsNullOrWhiteSpace(newInvoice.CardOwnerNationalID) && !consumer.ConsumerNationalID.Equals(newInvoice.CardOwnerNationalID, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new EntityConflictException(Messages.ConsumerNatIdIsNotEqTranNatId, "Consumer");
                }
            }

            newInvoice.Calculate();

            await invoiceService.CreateEntity(newInvoice);

            return new OperationResponse(Transactions.Shared.Messages.InvoiceCreated, StatusEnum.Success, newInvoice.InvoiceID);
        }

        [HttpPost]
        [Route("resend")]
        public async Task<ActionResult<OperationResponse>> Resend(ResendInvoiceRequest request)
        {
            if (!(request.InvoicesIDs?.Count() > 0))
            {
                return BadRequest(new OperationResponse(Messages.InvoicesForResendRequired, null, httpContextAccessor.TraceIdentifier, nameof(request.InvoicesIDs), Messages.InvoicesForResendRequired));
            }

            // TODO: different setting for invoice batch
            if (request.InvoicesIDs?.Count() > appSettings.TransmissionMaxBatchSize)
            {
                return BadRequest(new OperationResponse(string.Format(Messages.ProcesingItemsLimit, appSettings.TransmissionMaxBatchSize), null, HttpContext.TraceIdentifier, nameof(request.InvoicesIDs), string.Format(Messages.ProcesingItemsLimit, appSettings.TransmissionMaxBatchSize)));
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID));

            IEnumerable<Guid> invoicesToResend = null;

            using (var dbTransaction = invoiceService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
            {
                invoicesToResend = await invoiceService.StartSending(terminal.TerminalID, request.InvoicesIDs, dbTransaction);
                await dbTransaction.CommitAsync();
            }

            if (!(invoicesToResend?.Count() > 0))
            {
                return BadRequest(new OperationResponse(Messages.ThereAreNoInvoicesToResend, StatusEnum.Error));
            }

            foreach (var invoiceID in invoicesToResend)
            {
                await queue.PushToQueue(invoiceID);
            }

            var response = new OperationResponse
            {
                Status = StatusEnum.Success,
                Message = Messages.InvoicesQueuedForResend
            };

            return Ok(response);
        }

        /// <summary>
        /// Generate invocie in EasyInvoice system
        /// </summary>
        /// <param name="invoiceID"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Policy = Policy.AnyAdmin)]
        [HttpPost]
        [Route("generate/{invoiceID}")]
        public async Task<ActionResult<OperationResponse>> GenerateOrResendInvoice(Guid? invoiceID)
        {
            var dbInvoice = EnsureExists(await invoiceService.GetInvoices().FirstOrDefaultAsync(m => m.InvoiceID == invoiceID));

            if (dbInvoice.Status != Shared.Enums.InvoiceStatusEnum.Sending)
            {
                return BadRequest(new OperationResponse($"{Messages.InvoiceStateIsNotValid}", StatusEnum.Error, dbInvoice.InvoiceID, httpContextAccessor.TraceIdentifier));
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(dbInvoice.TerminalID.GetValueOrDefault()));

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            // invoice already generated
            if (!string.IsNullOrWhiteSpace(dbInvoice.InvoiceNumber))
            {
                if (string.IsNullOrWhiteSpace(dbInvoice.CopyDonwnloadUrl))
                {
                    return BadRequest(new OperationResponse($"{Messages.InvoiceStateIsNotValid}", StatusEnum.Error, dbInvoice.InvoiceID, httpContextAccessor.TraceIdentifier));
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(dbInvoice.DealDetails?.ConsumerEmail))
                    {
                        return BadRequest(new OperationResponse($"{Messages.EmailRequired}", StatusEnum.Error, dbInvoice.InvoiceID, httpContextAccessor.TraceIdentifier));
                    }
                    else
                    {
                        await emailSender.SendEmail(BuildInvoiceEmail(dbInvoice, terminal));

                        dbInvoice.Status = Shared.Enums.InvoiceStatusEnum.Sent;

                        await invoiceService.UpdateEntity(dbInvoice);

                        return new OperationResponse(Messages.InvoiceGenerated, StatusEnum.Success, dbInvoice.InvoiceID);
                    }
                }
            }
            else
            {
                var terminalProcessor = ValidateExists(
                    terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Invoicing),
                    Messages.InvoicingNotDefined);

                var terminalInvoicing = ValidateExists(
                   terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Invoicing),
                   Messages.InvoicingNotDefined);

                var invoicing = invoicingResolver.GetInvoicing(terminalInvoicing);

                var invoicingSettings = invoicingResolver.GetInvoicingTerminalSettings(terminalInvoicing, terminalInvoicing.Settings);

                try
                {
                    var invoicingRequest = mapper.Map<InvoicingCreateDocumentRequest>(dbInvoice);
                    invoicingRequest.InvoiceingSettings = invoicingSettings;

                    var invoicingResponse = await invoicing.CreateDocument(invoicingRequest);
                    mapper.Map(invoicingResponse, dbInvoice);

                    if (!invoicingResponse.Success)
                    {
                        logger.LogError($"Invoice generation failed. InvoiceID: {dbInvoice.InvoiceID}, response: {invoicingResponse.ErrorMessage}");

                        // TODO: UpdateEntityWithStatus
                        dbInvoice.Status = Shared.Enums.InvoiceStatusEnum.SendingFailed;
                        await invoiceService.UpdateEntity(dbInvoice);

                        return BadRequest(new OperationResponse($"{Messages.InvoiceGenerationFailed}: {invoicingResponse.ErrorMessage}", StatusEnum.Error, dbInvoice.InvoiceID, httpContextAccessor.TraceIdentifier, invoicingResponse.Errors));
                    }
                    else
                    {
                        dbInvoice.Status = Shared.Enums.InvoiceStatusEnum.Sent;

                        await invoiceService.UpdateEntity(dbInvoice);

                        return new OperationResponse(Messages.InvoiceGenerated, StatusEnum.Success, dbInvoice.InvoiceID);

                        // TODO: send CC
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Invoice generation failed. InvoiceID: {dbInvoice.InvoiceID}");

                    dbInvoice.Status = Shared.Enums.InvoiceStatusEnum.SendingFailed;
                    await invoiceService.UpdateEntity(dbInvoice);

                    return BadRequest(new OperationResponse($"{Messages.InvoiceGenerationFailed}", StatusEnum.Error, dbInvoice.InvoiceID, httpContextAccessor.TraceIdentifier));
                }
            }
        }

        private Email BuildInvoiceEmail(Invoice invoice, Terminal terminal)
        {
            var settings = terminal.InvoiceSettings;

            var emailSubject = invoice.InvoiceDetails?.InvoiceSubject ?? settings.DefaultInvoiceSubject;
            var emailTemplateCode = settings.InvoiceTemplateCode ?? "invoice";
            var substitutions = new List<TextSubstitution>();

            substitutions.Add(new TextSubstitution(nameof(invoice.InvoiceNumber), invoice.InvoiceNumber));
            substitutions.Add(new TextSubstitution(nameof(invoice.InvoiceDate), invoice.InvoiceDate.GetValueOrDefault().ToString("o")));
            substitutions.Add(new TextSubstitution(nameof(invoice.InvoiceAmount), invoice.InvoiceAmount.ToString("F2")));
            substitutions.Add(new TextSubstitution(nameof(invoice.CopyDonwnloadUrl), invoice.CopyDonwnloadUrl));
            substitutions.Add(new TextSubstitution(nameof(terminal.Merchant.MarketingName), terminal.Merchant.MarketingName ?? terminal.Merchant.BusinessName));

            var email = new Email
            {
                EmailTo = invoice.DealDetails?.ConsumerEmail,
                Subject = emailSubject,
                TemplateCode = emailTemplateCode,
                Substitutions = substitutions.ToArray()
            };

            return email;
        }
    }
}