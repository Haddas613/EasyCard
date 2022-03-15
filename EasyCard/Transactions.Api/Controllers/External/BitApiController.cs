using AutoMapper;
using Bit;
using Bit.Models;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api;
using Shared.Api.Attributes;
using Shared.Api.Configuration;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Validation;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.Queue;
using Shared.Helpers.Resources;
using Shared.Helpers.Services;
using Shared.Integration;
using Shared.Integration.Exceptions;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Extensions;
using Transactions.Api.Models.External;
using Transactions.Api.Models.External.Bit;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Services;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Transactions.Shared.Enums;
using SharedApi = Shared.Api;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Controllers.External
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    [Route("api/external/bit")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class BitApiController : ApiControllerBase
    {
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly IAggregatorResolver aggregatorResolver;
        private readonly ITransactionsService transactionsService;
        private readonly IMetricsService metrics;
        private readonly ITerminalsService terminalsService;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly BitProcessor bitProcessor;
        private readonly ISystemSettingsService systemSettingsService;
        private readonly ApiSettings apiSettings;
        private readonly IPaymentIntentService paymentIntentService;
        private readonly IPaymentRequestsService paymentRequestsService;
        private readonly IInvoiceService invoiceService;
        private readonly IQueue invoiceQueue;

        public BitApiController(
             IAggregatorResolver aggregatorResolver,
             ITransactionsService transactionService,
             ITerminalsService terminalsService,
             IMapper mapper,
             ILogger<BitApiController> logger,
             IMetricsService metrics,
             IHttpContextAccessorWrapper httpContextAccessor,
             BitProcessor bitProcessor,
             ISystemSettingsService systemSettingsService,
             IOptions<ApiSettings> apiSettings,
             IPaymentIntentService paymentIntentService,
             IPaymentRequestsService paymentRequestsService,
             IInvoiceService invoiceService,
             IQueueResolver queueResolver)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.aggregatorResolver = aggregatorResolver;
            this.transactionsService = transactionService;
            this.metrics = metrics;
            this.terminalsService = terminalsService;
            this.logger = logger;
            this.mapper = mapper;
            this.bitProcessor = bitProcessor;
            this.systemSettingsService = systemSettingsService;
            this.apiSettings = apiSettings.Value;
            this.paymentIntentService = paymentIntentService;
            this.paymentRequestsService = paymentRequestsService;
            this.invoiceService = invoiceService;
            this.invoiceQueue = queueResolver.GetQueue(QueueResolver.InvoiceQueue);
        }

        [HttpGet]
        [Route("get")]
        public async Task<ActionResult<BitTransactionResponse>> GetBitTransaction([FromQuery] GetBitTransactionQuery request)
        {
            //TODO: temporary
            var bitTransaction = await bitProcessor.GetBitTransaction(request.PaymentInitiationId, request.PaymentTransactionID.ToString(), Guid.NewGuid().ToString(), GetCorrelationID());

            return Ok(bitTransaction);
        }

        [HttpPost]
        [ValidateModelState]
        [Route("initial")]
        public async Task<ActionResult<InitialBitOperationResponse>> Initial([FromBody] CreateTransactionRequest model)
        {
            try
            {
                Terminal terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));

                // TODO: caching
                var systemSettings = await systemSettingsService.GetSystemSettings();

                // merge system settings with terminal settings
                mapper.Map(systemSettings, terminal);

                var terminalAggregator = ValidateExists(
                  terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator),
                  Transactions.Shared.Messages.AggregatorNotDefined);

                var processorSettings = new BitTerminalSettings();

                var bitProcessorConfig = terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.VirtualWalletProcessor);

                BitTerminalSettings bitSettings = null;

                if (bitProcessorConfig?.Settings != null)
                {
                    bitSettings = bitProcessorConfig.Settings.ToObject<BitTerminalSettings>();
                    if (bitSettings != null)
                    {
                        processorSettings = bitSettings;
                    }
                }

                //TODO: map terminal to model?
                if (model.VATRate == null)
                {
                    model.VATRate = terminal.Settings.VATRate;
                }

                var transaction = mapper.Map<PaymentTransaction>(model);

                transaction.SpecialTransactionType = SpecialTransactionTypeEnum.RegularDeal;
                transaction.JDealType = JDealTypeEnum.J4;
                transaction.CardPresence = CardPresenceEnum.CardNotPresent;
                transaction.DocumentOrigin = DocumentOriginEnum.Bit;
                transaction.PaymentRequestID = model.PaymentRequestID;
                transaction.PaymentIntentID = model.PaymentIntentID;
                transaction.BitTransactionDetails.BitMerchantNumber = bitSettings?.BitMerchantNumber;

                //if (transaction.ShvaTransactionDetails == null)
                //{
                //    transaction.ShvaTransactionDetails = new ShvaTransactionDetails();
                //}

                mapper.Map(terminal, transaction);

                transaction.VATRate = terminal.Settings.VATRate.GetValueOrDefault(0);
                transaction.DealDetails.UpdateDealDetails(null, terminal.Settings, transaction, transaction.CreditCardDetails);
                transaction.Calculate();

                await transactionsService.CreateEntity(transaction);
                metrics.TrackTransactionEvent(transaction, TransactionOperationCodesEnum.TransactionCreated);

                var processorRequest = mapper.Map<ProcessorCreateTransactionRequest>(transaction);

                processorRequest.ProcessorSettings = processorSettings;

                //processorRequest.RedirectURL = $"{apiSettings.CheckoutPortalUrl}/bit";

                var processorResponse = await bitProcessor.CreateTransaction(processorRequest);
                var bitResponse = processorResponse as BitCreateTransactionResponse;

                if (bitResponse is null)
                {
                    return BadRequest(new OperationResponse($"Bit error. Response is null ", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier));
                }

                transaction.BitTransactionDetails.BitPaymentInitiationId = bitResponse.PaymentInitiationId;
                transaction.BitTransactionDetails.BitTransactionSerialId = bitResponse.TransactionSerialId;
                await transactionsService.UpdateEntity(transaction);

                var aggregator = aggregatorResolver.GetAggregator(terminalAggregator);

                if (aggregator.ShouldBeProcessedByAggregator(transaction.TransactionType, transaction.SpecialTransactionType, transaction.JDealType))
                {
                    try
                    {
                        var aggregatorRequest = mapper.Map<AggregatorCreateTransactionRequest>(transaction);

                        //aggregatorRequest.CreditCardDetails = new SharedIntegration.Models.CreditCardDetails();
                        //mapper.Map(model, aggregatorRequest.CreditCardDetails);

                        if (string.IsNullOrWhiteSpace(aggregatorRequest.DealDetails.DealDescription))
                        {
                            //workaround for CH
                            aggregatorRequest.DealDetails.DealDescription = "-";
                        }

                        aggregatorRequest.TransactionDate = DateTime.Now;
                        var aggregatorSettings = aggregatorResolver.GetAggregatorTerminalSettings(terminalAggregator, terminalAggregator.Settings);
                        aggregatorRequest.AggregatorSettings = aggregatorSettings;
                        aggregatorRequest.IsBit = true;

                        var aggregatorValidationErrorMsg = aggregator.Validate(aggregatorRequest);
                        if (aggregatorValidationErrorMsg != null)
                        {
                            return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.RejectedByAggregator}: {aggregatorValidationErrorMsg}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier));
                        }

                        var aggregatorResponse = await aggregator.CreateTransaction(aggregatorRequest);
                        mapper.Map(aggregatorResponse, transaction);

                        if (!aggregatorResponse.Success)
                        {
                            await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedByAggregator, rejectionMessage: aggregatorResponse.ErrorMessage, rejectionReason: aggregatorResponse.RejectReasonCode);

                            return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.RejectedByAggregator}: {aggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, aggregatorResponse.Errors));
                        }
                        else
                        {
                            await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.ConfirmedByAggregator);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Aggregator Create Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToConfirmByAggregator, rejectionReason: RejectionReasonEnum.Unknown, rejectionMessage: ex.Message);

                        return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionStatusEnum.FailedToConfirmByAggregator.ToString(), (ex as IntegrationException)?.Message));
                    }
                }

                var response = new InitialBitOperationResponse(Transactions.Shared.Messages.TransactionCreated, StatusEnum.Success, transaction.PaymentTransactionID)
                {
                    BitPaymentInitiationId = transaction.BitTransactionDetails.BitPaymentInitiationId,
                    BitTransactionSerialId = transaction.BitTransactionDetails.BitTransactionSerialId,
                    RedirectURL = bitResponse.PaymentPageUrlAddress
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to initiate Transaction for Bit. TerminalID: {model.TerminalID}");
                return Ok(new OperationResponse(ex.Message, StatusEnum.Error));
            }
        }

        [HttpPost]
        [ValidateModelState]
        [Route("capture")]
        public async Task<ActionResult<OperationResponse>> Capture([FromBody] Models.External.Bit.CaptureBitTransactionRequest model)
        {
            try
            {
                var transaction = EnsureExists(await transactionsService.GetTransaction(t => t.PaymentTransactionID == model.PaymentTransactionID));

                Terminal terminal = EnsureExists(await terminalsService.GetTerminal(transaction.TerminalID));

                //already updated
                if (transaction.Status != TransactionStatusEnum.ConfirmedByAggregator && transaction.Status != TransactionStatusEnum.Initial)
                {
                    // in this case transaction should be manually resolved in CH
                    logger.LogError($"Failed to capture Transaction for Bit: transaction Status is invalid ({transaction.Status}). Transaction id: {transaction.PaymentTransactionID}");

                    return BadRequest(new OperationResponse(Shared.Messages.TransactionStatusIsNotValid, StatusEnum.Error, transaction.PaymentTransactionID));
                }

                var processorRequest = mapper.Map<ProcessorCreateTransactionRequest>(transaction);
                processorRequest.CorrelationId = transaction.CorrelationId ?? GetCorrelationID();
                bool sucessCapture = true;

                try
                {
                    var bitCaptureResponse = await bitProcessor.CaptureTransaction(processorRequest);

                    if (bitCaptureResponse == null || bitCaptureResponse.MessageCode != null)
                    {
                        logger.LogError($"Failed to capture Transaction for Bit: ({bitCaptureResponse?.MessageCode}). Transaction id: {transaction.PaymentTransactionID}");

                        sucessCapture = false;
                    }
                    else if (!string.IsNullOrEmpty(bitCaptureResponse.SuffixPlasticCardNumber))
                    {
                        transaction.CreditCardDetails = new Business.Entities.CreditCardDetails
                        {
                            CardNumber = $"000000000000{bitCaptureResponse.SuffixPlasticCardNumber}",
                        };
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to capture Transaction for Bit: ({ex.Message}). Transaction id: {transaction.PaymentTransactionID}");
                    sucessCapture = false;
                }

                return await PostProcessTransaction(transaction, terminal, sucessCapture);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to capture Transaction for Bit. Transaction id: {model.PaymentTransactionID}");
                return BadRequest(new OperationResponse($"Failed to capture Transaction for Bit. Transaction id: {model.PaymentTransactionID}", StatusEnum.Error));
            }
        }

        [HttpPost]
        [Route("cancelOrConfirmPending")]
        public async Task<ActionResult<OperationResponse>> CancelOrConfirmPending([FromQuery] Guid? transactionID)
        {
            var transaction = EnsureExists(await transactionsService.GetTransaction(t => t.PaymentTransactionID == transactionID));

            if (transaction.QuickStatus != QuickStatusFilterTypeEnum.Pending)
            {
                return new OperationResponse($"Transaction {transactionID} is in final state {transaction.Status}", StatusEnum.Error);
            }

            Terminal terminal = EnsureExists(await terminalsService.GetTerminal(transaction.TerminalID));

            return await PostProcessTransaction(transaction, terminal, true);
        }

        internal async Task<OperationResponse> RefundInternal(PaymentTransaction refundEntity)
        {
            var refundRequest = new BitRefundRequest
            {
                CreditAmount = refundEntity.TransactionAmount,
                ExternalSystemReference = refundEntity.InitialTransactionID.ToString(),
                RefundExternalSystemReference = refundEntity.PaymentTransactionID.ToString(),
                PaymentInitiationId = refundEntity.BitTransactionDetails.BitPaymentInitiationId,
            };

            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            var res = await bitProcessor.RefundBitTransaction(refundRequest, refundEntity.InitialTransactionID.ToString(), integrationMessageId, refundEntity.CorrelationId);

            refundEntity.BitTransactionDetails.RequestStatusCode = res.RequestStatusCode;
            refundEntity.BitTransactionDetails.RequestStatusDescription = res.RequestStatusDescription;

            if (res.Success)
            {
                await transactionsService.UpdateEntityWithStatus(refundEntity, TransactionStatusEnum.Completed);
                return new OperationResponse("Refund request created", StatusEnum.Success, refundEntity.PaymentTransactionID);
            }
            else
            {
                logger.LogWarning($"Refund attempt is failed for {refundEntity.PaymentTransactionID}: {res.RequestStatusCode} {res.RequestStatusDescription}");
                await transactionsService.UpdateEntityWithStatus(refundEntity, TransactionStatusEnum.RejectedByProcessor, rejectionMessage: res.RequestStatusDescription, rejectionReason: RejectionReasonEnum.Unknown);
                return new OperationResponse($"Refund attempt is failed for {refundEntity.PaymentTransactionID}: {res.RequestStatusCode} {res.RequestStatusDescription}", StatusEnum.Error, refundEntity.PaymentTransactionID);
            }
        }

        private async Task<ActionResult<OperationResponse>> ProcessByAggregator(PaymentTransaction transaction, SharedIntegration.ExternalSystems.IAggregator aggregator, object aggregatorSettings, bool success)
        {
            // reject to clearing house in case of bit error
            if (!success)
            {
                try
                {
                    var aggregatorRequest = mapper.Map<AggregatorCancelTransactionRequest>(transaction);
                    aggregatorRequest.AggregatorSettings = aggregatorSettings;
                    aggregatorRequest.RejectionReason = TransactionStatusEnum.FailedToConfirmByProcesor.ToString();
                    aggregatorRequest.IsBit = true;

                    var aggregatorResponse = await aggregator.CancelTransaction(aggregatorRequest);
                    mapper.Map(aggregatorResponse, transaction);

                    if (!aggregatorResponse.Success)
                    {
                        logger.LogError($"{nameof(BitApiController)}.{nameof(Capture)}: Aggregator Cancel Transaction request error. TransactionID: {transaction.PaymentTransactionID}");

                        await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.FailedToCancelByAggregator);

                        return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToCancelByAggregator}: {aggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier));
                    }
                    else
                    {
                        await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.CanceledByAggregator);

                        return new OperationResponse(Shared.Messages.CanceledByAggregator, StatusEnum.Error, transaction.PaymentTransactionID);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"{nameof(BitApiController)}.{nameof(Capture)}: Aggregator Cancel Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                    await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.FailedToCancelByAggregator);

                    return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionFinalizationStatusEnum.FailedToCancelByAggregator.ToString(), (ex as IntegrationException)?.Message));
                }
            }
            else // commit transaction in aggregator (Clearing House or upay)
            {
                try
                {
                    var commitAggregatorRequest = mapper.Map<AggregatorCommitTransactionRequest>(transaction);

                    commitAggregatorRequest.AggregatorSettings = aggregatorSettings;
                    commitAggregatorRequest.IsBit = true;

                    var commitAggregatorResponse = await aggregator.CommitTransaction(commitAggregatorRequest);
                    mapper.Map(commitAggregatorResponse, transaction);

                    if (!commitAggregatorResponse.Success)
                    {
                        logger.LogError($"{nameof(BitApiController)}.{nameof(Capture)}: Aggregator Commit Transaction request error. TransactionID: {transaction.PaymentTransactionID}");

                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToCommitByAggregator, rejectionMessage: commitAggregatorResponse.ErrorMessage, rejectionReason: commitAggregatorResponse.RejectReasonCode);

                        return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToCommitByAggregator}: {commitAggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, commitAggregatorResponse.Errors));
                    }
                    else
                    {
                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.Completed, transactionOperationCode: TransactionOperationCodesEnum.CommitedByAggregator);

                        return new OperationResponse(Shared.Messages.ConfirmedByAggregator, StatusEnum.Success, transaction.PaymentTransactionID);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"{nameof(BitApiController)}.{nameof(Capture)}: Aggregator Commit Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToCommitByAggregator, rejectionReason: RejectionReasonEnum.Unknown, rejectionMessage: ex.Message);

                    return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionStatusEnum.FailedToCommitByAggregator.ToString(), (ex as IntegrationException)?.Message));
                }
            }
        }

        private async Task CreateInvoice(PaymentTransaction transaction, Terminal terminal)
        {
            if (transaction.IssueInvoice == true && transaction.Currency == CurrencyEnum.ILS)
            {
                if (!string.IsNullOrWhiteSpace(transaction.DealDetails.ConsumerEmail) && !string.IsNullOrWhiteSpace(transaction.DealDetails.ConsumerName))
                {
                    using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
                    {
                        try
                        {
                            Invoice invoiceRequest = new Invoice();
                            mapper.Map(transaction, invoiceRequest);
                            invoiceRequest.InvoiceDetails = new SharedIntegration.Models.Invoicing.InvoiceDetails
                            {
                                InvoiceType = SharedIntegration.Models.Invoicing.InvoiceTypeEnum.Invoice,
                                InvoiceSubject = terminal.InvoiceSettings.DefaultInvoiceSubject,
                                SendCCTo = terminal.InvoiceSettings.SendCCTo
                            };

                            // in case if consumer name/natid is not specified in deal details, get it from credit card details
                            invoiceRequest.DealDetails = transaction.DealDetails;

                            invoiceRequest.MerchantID = terminal.MerchantID;

                            invoiceRequest.ApplyAuditInfo(httpContextAccessor);

                            invoiceRequest.Calculate();

                            await invoiceService.CreateEntity(invoiceRequest, dbTransaction: dbTransaction);

                            transaction.InvoiceID = invoiceRequest.InvoiceID;

                            await transactionsService.UpdateEntity(transaction, Transactions.Shared.Messages.InvoiceCreated, TransactionOperationCodesEnum.InvoiceCreated, dbTransaction: dbTransaction);

                            var invoicesToResend = await invoiceService.StartSending(terminal.TerminalID, new Guid[] { invoiceRequest.InvoiceID }, dbTransaction);

                            // TODO: validate, rollback
                            if (invoicesToResend.Count() > 0)
                            {
                                await invoiceQueue.PushToQueue(invoicesToResend.First());
                            }

                            await dbTransaction.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"{nameof(BitApiController)}.{nameof(Capture)}: Failed to create invoice. TransactionID: {transaction.PaymentTransactionID}");
                            await dbTransaction.RollbackAsync();
                        }
                    }
                }
            }
        }

        private async Task<ActionResult<OperationResponse>> PostProcessTransaction(PaymentTransaction transaction, Terminal terminal, bool sucessCapture)
        {
            ActionResult<OperationResponse> failedResponse = null;
            BitTransactionResponse bitTransaction = null;

            if (string.IsNullOrWhiteSpace(transaction.BitTransactionDetails.BitPaymentInitiationId))
            {
                sucessCapture = false;
                failedResponse = new OperationResponse($"Transaction {transaction.PaymentTransactionID} has no Bit details", StatusEnum.Error);
            }
            else
            {
                bitTransaction = await bitProcessor.GetBitTransaction(transaction.BitTransactionDetails.BitPaymentInitiationId, transaction.PaymentTransactionID.ToString(), Guid.NewGuid().ToString(), GetCorrelationID());

                if (bitTransaction == null)
                {
                    sucessCapture = false;
                    logger.LogError($"Failed to finalize Transaction for Bit: Bit deal {transaction.BitTransactionDetails.BitPaymentInitiationId} does not exist. Transaction id: {transaction.PaymentTransactionID}");
                    failedResponse = NotFound(new OperationResponse($"Bit deal {transaction.BitTransactionDetails.BitPaymentInitiationId} does not exist. Transaction id: {transaction.PaymentTransactionID}", StatusEnum.Error));
                }
                else if (bitTransaction.RequestStatusCodeResult == null)
                {
                    sucessCapture = false;
                    logger.LogError($"Failed to finalize Transaction for Bit: cannot parse Bit status {bitTransaction.RequestStatusCode} ({bitTransaction.RequestStatusDescription}). Transaction id: {transaction.PaymentTransactionID}");

                    failedResponse = BadRequest(new OperationResponse($"Transaction {transaction.PaymentTransactionID} cannot parse Bit status {bitTransaction.RequestStatusCode} ({bitTransaction.RequestStatusDescription})", StatusEnum.Error));
                }
                else if (!bitTransaction.RequestStatusCodeResult.Final)
                {
                    sucessCapture = false;
                    logger.LogError($"Failed to finalize Transaction for Bit: Bit state is not final {bitTransaction.RequestStatusCode} ({bitTransaction.RequestStatusDescription}). Transaction id: {transaction.PaymentTransactionID}");
                    failedResponse = BadRequest(new OperationResponse($"Transaction {transaction.PaymentTransactionID} Bit state is not final {bitTransaction.RequestStatusCode} ({bitTransaction.RequestStatusDescription})", StatusEnum.Error));
                }

                if (bitTransaction != null)
                {
                    transaction.BitTransactionDetails.RequestStatusCode = bitTransaction.RequestStatusCode;
                    transaction.BitTransactionDetails.RequestStatusDescription = bitTransaction.RequestStatusDescription;
                }
            }

            sucessCapture = sucessCapture && bitTransaction?.Success == true;

            if (sucessCapture)
            {
                await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.Completed);
            }
            else
            {
                // Introduce status: Rejected by Bit
                await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedByProcessor, TransactionFinalizationStatusEnum.Initial, rejectionMessage: bitTransaction?.RequestStatusDescription ?? "processing error", rejectionReason: RejectionReasonEnum.Unknown);
            }

            var terminalAggregator = ValidateExists(
               terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator),
               Transactions.Shared.Messages.AggregatorNotDefined);

            var aggregator = aggregatorResolver.GetAggregator(terminalAggregator);

            var aggregatorSettings = aggregatorResolver.GetAggregatorTerminalSettings(terminalAggregator, terminalAggregator.Settings);
            mapper.Map(aggregatorSettings, transaction);

            if (aggregator.ShouldBeProcessedByAggregator(transaction.TransactionType, transaction.SpecialTransactionType, transaction.JDealType))
            {
                var aggregatorResponseContent = await ProcessByAggregator(transaction, aggregator, aggregatorSettings, sucessCapture);
                var aggregatorResponse = aggregatorResponseContent.GetOperationResponse();

                if (aggregatorResponse?.Status != StatusEnum.Success)
                {
                    return aggregatorResponseContent;
                }
            }

            if (sucessCapture)
            {
                await CreateInvoice(transaction, terminal);
            }

            try
            {
                if (transaction.PaymentIntentID != null)
                {
                    await paymentIntentService.DeletePaymentIntent(transaction.PaymentIntentID.GetValueOrDefault());
                }
                else if (transaction.PaymentRequestID != null)
                {
                    var dbPaymentRequest = await paymentRequestsService.GetPaymentRequests().FirstOrDefaultAsync(m => m.PaymentRequestID == transaction.PaymentRequestID);

                    if (dbPaymentRequest != null)
                    {
                        if (dbPaymentRequest.Status == PaymentRequestStatusEnum.Payed || (int)dbPaymentRequest.Status < 0 || dbPaymentRequest.PaymentTransactionID != null)
                        {
                            logger.LogError($"Failed to finalize payment request/intent Transaction for Bit: Payment request {transaction.PaymentRequestID} status not valid ({dbPaymentRequest.Status}). Transaction id: {transaction.PaymentTransactionID}");
                        }
                        else
                        {
                            await paymentRequestsService.UpdateEntityWithStatus(dbPaymentRequest, sucessCapture ? PaymentRequestStatusEnum.Payed : PaymentRequestStatusEnum.PaymentFailed, paymentTransactionID: transaction.PaymentTransactionID, message: Transactions.Shared.Messages.PaymentRequestPaymentSuccessed);
                        }
                    }
                    else
                    {
                        logger.LogError($"Failed to finalize payment request/intent Transaction for Bit: Payment request {transaction.PaymentRequestID} not found. Transaction id: {transaction.PaymentTransactionID}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to finalize payment request/intent Transaction for Bit: ({ex.Message}). Transaction id: {transaction.PaymentTransactionID}");
            }

            if (sucessCapture)
            {
                return new OperationResponse(Shared.Messages.TransactionUpdated, StatusEnum.Success, transaction.PaymentTransactionID);
            }
            else
            {
                if (failedResponse != null)
                {
                    return failedResponse;
                }
                else
                {
                    return BadRequest(new OperationResponse($"{Shared.Messages.BitPaymentFailed}: {bitTransaction?.RequestStatusDescription}", StatusEnum.Error, transaction.PaymentTransactionID));
                }
            }
        }
    }
}