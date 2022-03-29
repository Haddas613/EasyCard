﻿using System;
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
using Z.EntityFramework.Plus;
using Merchants.Business.Entities.Terminal;
using Transactions.Shared.Enums;
using Transactions.Api.Extensions;
using Newtonsoft.Json;
using Transactions.Api.Validation;
using Shared.Api.Swagger;
using SharedIntegration = Shared.Integration;
using SharedApi = Shared.Api;

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
        private readonly ITransactionsService transactionsService;
        private readonly BasicServices.Services.IExcelService excelService;

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
                    IEmailSender emailSender,
                    ITransactionsService transactionsService,
                    BasicServices.Services.IExcelService excelService)
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
            this.transactionsService = transactionsService;
            this.excelService = excelService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = (httpContextAccessor.GetUser().IsAdmin() ? typeof(InvoiceSummaryAdmin) : typeof(InvoiceSummary))
                    .GetObjectMeta(InvoiceSummaryResource.ResourceManager, CurrentCulture)
            };
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<InvoiceSummary>>> GetInvoices([FromQuery] InvoicesFilter filter)
        {
            var query = invoiceService.GetInvoices().Filter(filter);
            var numberOfRecordsFuture = query.DeferredCount().FutureValue();

            using (var dbTransaction = invoiceService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                if (httpContextAccessor.GetUser().IsAdmin())
                {
                    var response = new SummariesResponse<InvoiceSummaryAdmin>();

                    var summary = await mapper.ProjectTo<InvoiceSummaryAdmin>(query.OrderByDynamic(filter.SortBy ?? nameof(Invoice.InvoiceID), filter.SortDesc).ApplyPagination(filter)).Future().ToListAsync();

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
                    var response = new SummariesResponse<InvoiceSummary>();

                    response.Data = await mapper.ProjectTo<InvoiceSummary>(query.OrderByDynamic(filter.SortBy ?? nameof(Invoice.InvoiceID), filter.SortDesc).ApplyPagination(filter)).Future().ToListAsync();
                    response.NumberOfRecords = numberOfRecordsFuture.Value;
                    return Ok(response);
                }
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

                invoice.TerminalName = await terminalsService.GetTerminals().Where(t => t.TerminalID == invoice.TerminalID).Select(t => t.Label).FirstOrDefaultAsync();

                return Ok(invoice);
            }
        }

        // TODO: support several download urls
        [HttpGet]
        [Route("{invoiceID}/download")]
        public async Task<ActionResult<DownloadInvoiceResponse>> GetInvoiceDownloadURL([FromRoute] Guid invoiceID)
        {
            using (var dbTransaction = invoiceService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var dbInvoice = EnsureExists(await invoiceService.GetInvoices().Where(m => m.InvoiceID == invoiceID).FirstOrDefaultAsync());

                if (dbInvoice.ExternalSystemData == null || dbInvoice.ExternalSystemData.Count == 0)
                {
                    var downloadUrl = new List<string> { dbInvoice.DownloadUrl };

                    return new DownloadInvoiceResponse(downloadUrl) { Status = StatusEnum.Success, EntityUID = invoiceID };
                }
                else
                {
                    var terminal = EnsureExists(await terminalsService.GetTerminal(dbInvoice.TerminalID));

                    // TODO: caching
                    var systemSettings = await systemSettingsService.GetSystemSettings();

                    // merge system settings with terminal settings
                    mapper.Map(systemSettings, terminal);

                    var terminalInvoicing = terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Invoicing);

                    if (terminalInvoicing == null)
                    {
                        dbInvoice.Status = Shared.Enums.InvoiceStatusEnum.SendingFailed;
                        await invoiceService.UpdateEntity(dbInvoice);

                        throw new BusinessException(Messages.InvoicingNotDefined);
                    }

                    var invoicing = invoicingResolver.GetInvoicing(terminalInvoicing);
                    var invoicingSettings = invoicingResolver.GetInvoicingTerminalSettings(terminalInvoicing, terminalInvoicing.Settings);

                    try
                    {
                        var invoicingRequest = mapper.Map<InvoicingCreateDocumentRequest>(dbInvoice);
                        invoicingRequest.InvoiceingSettings = invoicingSettings;

                        var invoicingResponse = await invoicing.GetDownloadUrls(dbInvoice.ExternalSystemData, invoicingSettings);

                        return new DownloadInvoiceResponse(invoicingResponse) { Status = StatusEnum.Success, EntityUID = invoiceID };
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Invoice get download Url failed. InvoiceID: {dbInvoice.InvoiceID}");

                        return BadRequest(new OperationResponse($"Invoice get download Url failed", StatusEnum.Error, dbInvoice.InvoiceID, httpContextAccessor.TraceIdentifier));
                    }
                }
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

            // TODO: validate Invoice with Payment Info, do not send to EInvoice if no payment info present
            InvoiceValidator.ValidateInvoiceRequest(model);

            model.Calculate(terminal.Settings.VATRate.GetValueOrDefault(0));

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
            var consumer = newInvoice.DealDetails.ConsumerID != null ? EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == newInvoice.DealDetails.ConsumerID), "Consumer") : null;

            newInvoice.DealDetails.CheckConsumerDetails(consumer);

            newInvoice.DealDetails.UpdateDealDetails(consumer, terminal.Settings, newInvoice, null);

            var creditCardDetails = newInvoice.PaymentDetails.FirstOrDefault(d => d.PaymentType == SharedIntegration.Models.PaymentTypeEnum.Card) as SharedIntegration.Models.PaymentDetails.CreditCardPaymentDetails;

            // in case if consumer name/natid is not specified in deal details, get it from credit card details
            newInvoice.DealDetails.UpdateDealDetails(creditCardDetails);

            newInvoice.Calculate();

            // TODO: check result
            await invoiceService.CreateEntity(newInvoice);

            var invoicesToResend = await invoiceService.StartSending(terminal.TerminalID, new Guid[] { newInvoice.InvoiceID }, null);

            // TODO: validate
            if (invoicesToResend.Count() > 0)
            {
                await queue.PushToQueue(invoicesToResend.First());
            }

            return new OperationResponse(Transactions.Shared.Messages.InvoiceCreated, StatusEnum.Success, newInvoice.InvoiceID);
        }

        [HttpPost("transaction/{transactionID:guid}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> CreateInvoiceForTransaction([FromRoute]Guid transactionID)
        {
            var merchantID = User.GetMerchantID();
            var userIsTerminal = User.IsTerminal();

            var transaction = EnsureExists(await transactionsService.GetTransaction(t => t.PaymentTransactionID == transactionID));

            if (transaction.InvoiceID != null)
            {
                return BadRequest(Messages.TransactionAlreadyHasInvoice);
            }

            if ((int)transaction.Status < 0)
            {
                return BadRequest(Messages.OnlySuccessfulTransactionsAreAllowed);
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(transaction.TerminalID));

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            var endResponse = new OperationResponse();

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
            {
                try
                {
                    Invoice invoiceRequest = new Invoice();
                    mapper.Map(transaction, invoiceRequest);

                    //TODO: Handle refund & credit default invoice types
                    InvoiceDetails invoiceDetails = new InvoiceDetails { InvoiceType = terminal.InvoiceSettings.DefaultInvoiceType.GetValueOrDefault() };
                    invoiceDetails.UpdateInvoiceDetails(terminal.InvoiceSettings, transaction);

                    invoiceRequest.InvoiceDetails = invoiceDetails;

                    invoiceRequest.MerchantID = terminal.MerchantID;

                    invoiceRequest.ApplyAuditInfo(httpContextAccessor);

                    await invoiceService.CreateEntity(invoiceRequest, dbTransaction: dbTransaction);

                    endResponse = new OperationResponse(Messages.InvoiceCreated, StatusEnum.Success, invoiceRequest.InvoiceID);

                    transaction.InvoiceID = invoiceRequest.InvoiceID;

                    await transactionsService.UpdateEntity(transaction, Messages.InvoiceCreated, TransactionOperationCodesEnum.InvoiceCreated, dbTransaction: dbTransaction);

                    var invoicesToResend = await invoiceService.StartSending(terminal.TerminalID, new Guid[] { invoiceRequest.InvoiceID }, dbTransaction);

                    // TODO: validate, rollback
                    if (invoicesToResend.Count() > 0)
                    {
                        await queue.PushToQueue(invoicesToResend.First());
                    }

                    await dbTransaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to create invoice. TransactionID: {transaction.PaymentTransactionID}");

                    endResponse = new OperationResponse($"{Messages.FailedToCreateInvoice}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, "FailedToCreateInvoice", ex.Message);

                    await dbTransaction.RollbackAsync();
                }
            }

            return endResponse;
        }

        [HttpPost]
        [Route("resend-admin")]
        [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
        public async Task<ActionResult<OperationResponse>> ResendInvoicesAdmin(ResendInvoiceRequest resendInvoiceRequest)
        {
            if (resendInvoiceRequest.InvoicesIDs == null || resendInvoiceRequest.InvoicesIDs.Count() == 0)
            {
                return BadRequest(new OperationResponse(Messages.TransactionsForTransmissionRequired, null, httpContextAccessor.TraceIdentifier, nameof(resendInvoiceRequest.InvoicesIDs), Messages.TransactionsForTransmissionRequired));
            }

            if (resendInvoiceRequest.InvoicesIDs.Count() > appSettings.TransmissionMaxBatchSize)
            {
                return BadRequest(new OperationResponse(string.Format(Messages.TransmissionLimit, appSettings.TransmissionMaxBatchSize), null, httpContextAccessor.TraceIdentifier, nameof(resendInvoiceRequest.InvoicesIDs), string.Format(Messages.TransmissionLimit, appSettings.TransmissionMaxBatchSize)));
            }

            var transactionTerminals = (await invoiceService.GetInvoices()
                .Where(t => resendInvoiceRequest.InvoicesIDs.Contains(t.InvoiceID))
                .ToListAsync())
                .GroupBy(k => k.TerminalID, v => v.InvoiceID)
                .ToDictionary(k => k.Key, v => v.ToList());

            var response = new OperationResponse
            {
                Status = StatusEnum.Success,
                Message = Messages.InvoicesQueuedForResend
            };

            foreach (var batch in transactionTerminals)
            {
                await Resend(new ResendInvoiceRequest { TerminalID = batch.Key, InvoicesIDs = batch.Value });
            }

            return Ok(response);
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
        /// <param name="ignoreStatus"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Policy = Policy.AnyAdmin)]
        [HttpPost]
        [Route("generate/{invoiceID}")]
        public async Task<ActionResult<OperationResponse>> GenerateOrResendInvoice(Guid? invoiceID, [FromQuery]bool ignoreStatus = false)
        {
            var dbInvoice = EnsureExists(await invoiceService.GetInvoices().FirstOrDefaultAsync(m => m.InvoiceID == invoiceID));

            if (!ignoreStatus && dbInvoice.Status != Shared.Enums.InvoiceStatusEnum.Sending)
            {
                return BadRequest(new OperationResponse($"{Messages.InvoiceStateIsNotValid}", StatusEnum.Error, dbInvoice.InvoiceID, httpContextAccessor.TraceIdentifier));
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(dbInvoice.TerminalID));

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            // invoice already generated
            if (!string.IsNullOrWhiteSpace(dbInvoice.InvoiceNumber) && !ignoreStatus)
            {
                // TODO: support for R1 invoices
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
                        await SendInvoiceEmail(dbInvoice.DealDetails?.ConsumerEmail, dbInvoice, terminal);

                        dbInvoice.Status = Shared.Enums.InvoiceStatusEnum.Sent;

                        await invoiceService.UpdateEntity(dbInvoice);

                        return new OperationResponse(Messages.InvoiceGenerated, StatusEnum.Success, dbInvoice.InvoiceID);
                    }
                }
            }
            else
            {
                var terminalInvoicing = terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Invoicing);

                if (terminalInvoicing == null)
                {
                    dbInvoice.Status = Shared.Enums.InvoiceStatusEnum.SendingFailed;
                    await invoiceService.UpdateEntity(dbInvoice);

                    throw new BusinessException(Messages.InvoicingNotDefined);
                }

                var invoicing = invoicingResolver.GetInvoicing(terminalInvoicing);
                var invoicingSettings = invoicingResolver.GetInvoicingTerminalSettings(terminalInvoicing, terminalInvoicing.Settings);
                dbInvoice.InvoicingID = terminalInvoicing.ExternalSystemID;

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

                        if (dbInvoice.InvoiceDetails.SendCCTo?.Any() == true)
                        {
                            foreach (var cc in dbInvoice.InvoiceDetails.SendCCTo)
                            {
                                await SendInvoiceEmail(cc, dbInvoice, terminal);
                            }
                        }

                        //Send to customer as well
                        if (dbInvoice.DealDetails.ConsumerEmail != null)
                        {
                            await SendInvoiceEmail(dbInvoice.DealDetails.ConsumerEmail, dbInvoice, terminal);
                        }

                        return new OperationResponse(Messages.InvoiceGenerated, StatusEnum.Success, dbInvoice.InvoiceID);
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

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("{invoiceID}/history")]
        [Authorize(Policy = Policy.AnyAdmin)]
        public async Task<ActionResult<SummariesResponse<InvoiceHistoryResponse>>> GetInvoiceHistory([FromRoute] Guid invoiceID)
        {
            var dbInvoice = EnsureExists(await invoiceService.GetInvoices().FirstOrDefaultAsync(m => m.InvoiceID == invoiceID));

            var query = invoiceService.GetInvoiceHistory(invoiceID);

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var numberOfRecords = query.DeferredCount().FutureValue();
                var response = new SummariesResponse<InvoiceHistoryResponse>();

                response.Data = await mapper.ProjectTo<InvoiceHistoryResponse>(query.OrderByDescending(t => t.OperationDate)).Future().ToListAsync();
                response.NumberOfRecords = numberOfRecords.Value;

                return Ok(response);
            }
        }

        /// <summary>
        /// Create customer in Invoicing system
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Policy = Policy.AnyAdmin)]
        [HttpPost]
        [Route("createConsumer")]
        public async Task<ActionResult<CreateInvoicingConsumerResponse>> CreateInvoicingConsumer(CreateInvoicingConsumerRequest consumerRequest)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(consumerRequest.TerminalID));

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            var terminalInvoicing = terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Invoicing);

            if (terminalInvoicing == null)
            {
                return new CreateInvoicingConsumerResponse { Message = Messages.CannotCreateInvoicingConsumer, Status = StatusEnum.Error };
            }

            var invoicing = invoicingResolver.GetInvoicing(terminalInvoicing);

            if (!invoicing.CanCreateConsumer())
            {
                return new CreateInvoicingConsumerResponse { Message = Messages.CannotCreateInvoicingConsumer, Status = StatusEnum.Error };
            }

            var invoicingSettings = invoicingResolver.GetInvoicingTerminalSettings(terminalInvoicing, terminalInvoicing.Settings);

            try
            {
                var invoicingRequest = new CreateConsumerRequest
                {
                     CellPhone = consumerRequest.CellPhone,
                     ConsumerName = consumerRequest.ConsumerName,
                     Email = consumerRequest.Email,
                     NationalID = consumerRequest.NationalID
                };
                invoicingRequest.InvoiceingSettings = invoicingSettings;

                var invoicingResponse = await invoicing.CreateConsumerOrGetExisting(invoicingRequest);

                if (!invoicingResponse.Success)
                {
                    logger.LogError($"Cannot create invoicing consumer. ConsumerID: {consumerRequest.ConsumerID}, response: {invoicingResponse.ErrorMessage}");

                    return BadRequest(new OperationResponse($"{Messages.CannotCreateInvoicingConsumer}: {invoicingResponse.ErrorMessage}", StatusEnum.Error, consumerRequest.ConsumerID));
                }
                else
                {
                    return new CreateInvoicingConsumerResponse { ConsumerReference = invoicingResponse.ConsumerReference, Origin = invoicingResponse.Origin };
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Cannot create invoicing consumer. ConsumerID: {consumerRequest.ConsumerID}, response: {ex.Message}");

                return BadRequest(new OperationResponse($"{Messages.CannotCreateInvoicingConsumer}", StatusEnum.Error, consumerRequest.ConsumerID));
            }
        }

        [HttpGet]
        [Route("$excel")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<OperationResponse>> GetInvoicesExcel([FromQuery] InvoicesFilter filter)
        {
            var query = invoiceService.GetInvoices().Filter(filter);

            using (var dbTransaction = invoiceService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                if (httpContextAccessor.GetUser().IsAdmin())
                {
                    var response = new SummariesResponse<InvoiceSummaryAdmin>();

                    var summary = await mapper.ProjectTo<InvoiceSummaryAdmin>(query.OrderByDynamic(filter.SortBy ?? nameof(Invoice.InvoiceTimestamp), filter.SortDesc)).ToListAsync();

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

                    var mapping = InvoiceSummaryResource.ResourceManager.GetExcelColumnNames<InvoiceSummaryAdmin>();
                    var res = await excelService.GenerateFile($"Admin/Invoices-{Guid.NewGuid()}.xlsx", "Invoices", summary, mapping);
                    return Ok(new OperationResponse { Status = StatusEnum.Success, EntityReference = res });
                }
                else
                {
                    var data = await mapper.ProjectTo<InvoiceSummary>(query.OrderByDynamic(filter.SortBy ?? nameof(Invoice.InvoiceTimestamp), filter.SortDesc)).ToListAsync();
                    var mapping = InvoiceSummaryResource.ResourceManager.GetExcelColumnNames<InvoiceSummary>();
                    var res = await excelService.GenerateFile($"{User.GetMerchantID()}/Invoices-{Guid.NewGuid()}.xlsx", "Invoices", data, mapping);
                    return Ok(new OperationResponse { Status = StatusEnum.Success, EntityReference = res });
                }
            }
        }

        [NonAction]
        public async Task SendInvoiceEmail(string emailTo, Invoice invoice, Terminal terminal)
        {
            var settings = terminal.InvoiceSettings;

            var emailSubject = invoice.InvoiceDetails?.InvoiceSubject ?? settings.DefaultInvoiceSubject;
            var emailTemplateCode = settings.EmailTemplateCode ?? nameof(Invoice);
            var substitutions = new List<TextSubstitution>();

            substitutions.Add(new TextSubstitution(nameof(invoice.InvoiceNumber), invoice.InvoiceNumber));
            substitutions.Add(new TextSubstitution(nameof(invoice.InvoiceDate), invoice.InvoiceDate.GetValueOrDefault().ToString("d"))); // TODO: locale
            substitutions.Add(new TextSubstitution(nameof(invoice.InvoiceAmount), $"{invoice.InvoiceAmount.ToString("F2")}{invoice.Currency.GetCurrencySymbol()}"));

            var invoiceDownloadUrl = invoice.CopyDonwnloadUrl;
            if (invoiceDownloadUrl == null)
            {
                var actionResult = (await GetInvoiceDownloadURL(invoice.InvoiceID)).GetResult();
                invoiceDownloadUrl = actionResult.DownloadLinks?.FirstOrDefault();
            }

            substitutions.Add(new TextSubstitution(nameof(invoice.CopyDonwnloadUrl), invoiceDownloadUrl));
            substitutions.Add(new TextSubstitution(nameof(terminal.Merchant.MarketingName), terminal.Merchant.MarketingName ?? terminal.Merchant.BusinessName));

            var email = new Email
            {
                EmailTo = emailTo,
                Subject = emailSubject,
                TemplateCode = emailTemplateCode,
                Substitutions = substitutions.ToArray()
            };

            await emailSender.SendEmail(email);
        }
    }
}