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
using Shared.Helpers.Events;
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
        private readonly IEventsService events;
        private readonly ITerminalsService terminalsService;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly BitProcessor bitProcessor;
        private readonly ISystemSettingsService systemSettingsService;
        private readonly ApiSettings apiSettings;
        private readonly IPaymentIntentService paymentIntentService;
        private readonly IPaymentRequestsService paymentRequestsService;
        private readonly InvoicingController invoicingController;

        public BitApiController(
             IAggregatorResolver aggregatorResolver,
             ITransactionsService transactionService,
             ITerminalsService terminalsService,
             IMapper mapper,
             ILogger<BitApiController> logger,
             IEventsService events,
             IHttpContextAccessorWrapper httpContextAccessor,
             BitProcessor bitProcessor,
             ISystemSettingsService systemSettingsService,
             IOptions<ApiSettings> apiSettings,
             IPaymentIntentService paymentIntentService,
             IPaymentRequestsService paymentRequestsService,
             InvoicingController invoicingController)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.aggregatorResolver = aggregatorResolver;
            this.transactionsService = transactionService;
            this.events = events;
            this.terminalsService = terminalsService;
            this.logger = logger;
            this.mapper = mapper;
            this.bitProcessor = bitProcessor;
            this.systemSettingsService = systemSettingsService;
            this.apiSettings = apiSettings.Value;
            this.paymentIntentService = paymentIntentService;
            this.paymentRequestsService = paymentRequestsService;
            this.invoicingController = invoicingController;
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

                // payment request/intent
                PaymentRequest dbPaymentRequest = null;

                if (model.PaymentRequestID != null)
                {
                    dbPaymentRequest = EnsureExists(await paymentRequestsService.GetPaymentRequests().FirstOrDefaultAsync(m => m.PaymentRequestID == model.PaymentRequestID));

                    if (dbPaymentRequest.Status == PaymentRequestStatusEnum.Payed || (int)dbPaymentRequest.Status < 0 || dbPaymentRequest.PaymentTransactionID != null)
                    {
                        return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.PaymentRequestStatusIsClosed}", StatusEnum.Error, dbPaymentRequest.PaymentRequestID, httpContextAccessor.TraceIdentifier));
                    }
                }
                else if (model.PaymentIntentID != null)
                {
                    dbPaymentRequest = EnsureExists(await paymentIntentService.GetPaymentIntent(model.PaymentIntentID.GetValueOrDefault()), "PaymentIntent");
                }

                if (dbPaymentRequest != null)
                {
                    mapper.Map(dbPaymentRequest, model);
                }

                var transaction = mapper.Map<PaymentTransaction>(model);

                transaction.ApplyAuditInfo(httpContextAccessor);

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

                var processorRequest = mapper.Map<ProcessorCreateTransactionRequest>(transaction);

                processorRequest.ProcessorSettings = processorSettings;

                processorRequest.DealDescription = terminal.Merchant.MarketingName ?? terminal.Merchant.BusinessName;

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

                bool sucessCapture = await CaptureInternal(transaction);

                var bitTransaction = await bitProcessor.GetBitTransaction(transaction.BitTransactionDetails.BitPaymentInitiationId, transaction.PaymentTransactionID.ToString(), Guid.NewGuid().ToString(), GetCorrelationID());

                return await PostProcessTransaction(transaction, terminal, sucessCapture, bitTransaction);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to capture Transaction for Bit. Transaction id: {model.PaymentTransactionID}");
                return BadRequest(new OperationResponse($"Failed to capture Transaction for Bit. Transaction id: {model.PaymentTransactionID}", StatusEnum.Error));
            }
        }

        private async Task<bool> CaptureInternal(PaymentTransaction transaction)
        {
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

            return sucessCapture;
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

            var bitTransaction = string.IsNullOrWhiteSpace(transaction.BitTransactionDetails?.BitPaymentInitiationId) ? null : await bitProcessor.GetBitTransaction(transaction.BitTransactionDetails.BitPaymentInitiationId, transaction.PaymentTransactionID.ToString(), Guid.NewGuid().ToString(), GetCorrelationID());

            if (bitTransaction != null)
            {
                bool successCapture = true;

                if (bitTransaction.RequestStatusCodeResult?.BitRequestStatusCode == BitRequestStatusCodeEnum.CreditExtensionPerformed)
                {
                    successCapture = await CaptureInternal(transaction);

                    bitTransaction = await bitProcessor.GetBitTransaction(transaction.BitTransactionDetails.BitPaymentInitiationId, transaction.PaymentTransactionID.ToString(), Guid.NewGuid().ToString(), GetCorrelationID());
                }

                return await PostProcessTransaction(transaction, terminal, successCapture, bitTransaction);
            }
            else
            {
                await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedByProcessor, transactionOperationCode: TransactionOperationCodesEnum.RejectedByProcessor);
                return new OperationResponse($"Bit transaction for {transactionID} does exist", StatusEnum.Error);
            }
        }

        internal async Task<OperationResponse> RefundInternal(PaymentTransaction transaction, Terminal terminal, ChargebackRequest request)
        {
            if (string.IsNullOrWhiteSpace(transaction.BitTransactionDetails?.BitPaymentInitiationId) || transaction.DocumentOrigin != DocumentOriginEnum.Bit)
            {
                return new OperationResponse($"It is possible to make refund only for Bit transactions", StatusEnum.Error);
            }

            if (transaction.Status != TransactionStatusEnum.Completed && transaction.Status != TransactionStatusEnum.Chargeback)
            {
                return new OperationResponse($"It is possible to make refund only for completed or partially refunded Bit transactions", StatusEnum.Error);
            }

            if (transaction.Amount < request.RefundAmount + transaction.TotalRefund)
            {
                return new OperationResponse($"It is possible to make refund only for amount less than or equal to {transaction.Amount}", StatusEnum.Error);
            }

            PaymentTransaction refundEntity = new PaymentTransaction();
            refundEntity.TerminalID = transaction.TerminalID;
            refundEntity.MerchantID = transaction.MerchantID;
            refundEntity.InitialTransactionID = transaction.PaymentTransactionID;
            refundEntity.SpecialTransactionType = SpecialTransactionTypeEnum.Refund;
            refundEntity.Status = TransactionStatusEnum.Initial;
            refundEntity.TransactionAmount = request.RefundAmount;
            refundEntity.DealDetails = ReflectionHelpers.Clone(transaction.DealDetails);
            refundEntity.BitTransactionDetails = ReflectionHelpers.Clone(transaction.BitTransactionDetails);
            refundEntity.IssueInvoice = transaction.IssueInvoice;
            refundEntity.VATRate = transaction.VATRate;
            refundEntity.DocumentOrigin = DocumentOriginEnum.Bit; // really it can be UI but "Bit" used for consistency in search
            refundEntity.CreditCardDetails = ReflectionHelpers.Clone(transaction.CreditCardDetails);

            refundEntity.ApplyAuditInfo(httpContextAccessor);

            refundEntity.Calculate();

            await transactionsService.CreateEntity(refundEntity);

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

            if (!res.Success)
            {
                logger.LogWarning($"Refund attempt is failed for {refundEntity.PaymentTransactionID}: {res.RequestStatusCode} {res.RequestStatusDescription}");
                await transactionsService.UpdateEntityWithStatus(refundEntity, TransactionStatusEnum.RejectedByProcessor, rejectionMessage: res.RequestStatusDescription, rejectionReason: RejectionReasonEnum.Unknown);
                return new OperationResponse($"Refund attempt is failed for {refundEntity.PaymentTransactionID}: {res.RequestStatusCode} {res.RequestStatusDescription}", StatusEnum.Error, refundEntity.PaymentTransactionID);
            }

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
            {
                try
                {
                    await transactionsService.UpdateEntityWithStatus(refundEntity, TransactionStatusEnum.Completed);

                    transaction.TotalRefund = transaction.TotalRefund.GetValueOrDefault() + request.RefundAmount;

                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.Chargeback, transactionOperationCode: TransactionOperationCodesEnum.RefundCreated);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"{nameof(BitApiController)}.{nameof(RefundInternal)}: Failed to commitd db transaction. TransactionID: {transaction.PaymentTransactionID}");
                    await dbTransaction.RollbackAsync();
                }
            }

            // TODO: inner response
            await CreateInvoice(refundEntity, terminal);

            return new OperationResponse("Refund request created", StatusEnum.Success, refundEntity.PaymentTransactionID);
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

        // TODO: return
        private async Task CreateInvoice(PaymentTransaction transaction, Terminal terminal)
        {
            var invoiceDetails = new SharedIntegration.Models.Invoicing.InvoiceDetails
            {
                InvoiceType = terminal.InvoiceSettings.DefaultInvoiceType.GetValueOrDefault(),
                InvoiceSubject = terminal.InvoiceSettings.DefaultInvoiceSubject,
                SendCCTo = terminal.InvoiceSettings.SendCCTo
            };

            if (transaction.SpecialTransactionType == SharedIntegration.Models.SpecialTransactionTypeEnum.Refund)
            {
                invoiceDetails.InvoiceType = terminal.InvoiceSettings.DefaultRefundInvoiceType.GetValueOrDefault();
            }

            await invoicingController.ProcessInvoice(terminal, transaction, invoiceDetails);
        }

        private async Task<ActionResult<OperationResponse>> PostProcessTransaction(PaymentTransaction transaction, Terminal terminal, bool sucessCapture, BitTransactionResponse bitTransaction)
        {
            ActionResult<OperationResponse> failedResponse = null;

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

                await bitProcessor.DeleteBitTransaction(transaction.BitTransactionDetails.BitPaymentInitiationId, transaction.PaymentTransactionID.ToString(), Guid.NewGuid().ToString(), GetCorrelationID());
            }

            if (bitTransaction != null)
            {
                transaction.BitTransactionDetails.RequestStatusCode = bitTransaction.RequestStatusCode;
                transaction.BitTransactionDetails.RequestStatusDescription = bitTransaction.RequestStatusDescription;
            }

            sucessCapture = sucessCapture && bitTransaction?.Success == true;

            if (sucessCapture)
            {
                await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.Completed);
            }
            else
            {
                // TODO: Introduce status: Rejected by Bit
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
                    _ = events.RaiseTransactionEvent(transaction, CustomEvent.TransactionRejected, $"{Shared.Messages.FailedToConfirmByAggregator}: {aggregatorResponse.Message}");
                    return aggregatorResponseContent;
                }
            }

            if (sucessCapture)
            {
                await CreateInvoice(transaction, terminal);
            }

            if (sucessCapture)
            {
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

                _ = events.RaiseTransactionEvent(transaction, CustomEvent.TransactionCreated);
                return new OperationResponse(Shared.Messages.PaymentWasCompletedSuccessfully, StatusEnum.Success, transaction.PaymentTransactionID);
            }
            else
            {
                if (failedResponse != null)
                {
                    var failedRes = failedResponse.GetOperationResponse();
                    _ = events.RaiseTransactionEvent(transaction, CustomEvent.TransactionRejected, $"{Shared.Messages.FailedToProcessTransaction}: {failedRes.Message}");
                    return failedResponse;
                }
                else
                {
                    string message = bitTransaction?.RequestStatusCode switch
                    {
                        "3" => Shared.Messages.PaymentWasNotCompleted,
                        "11" => Shared.Messages.PaymentWasCompletedSuccessfully,
                        _ => $"{Shared.Messages.BitPaymentFailed}: {bitTransaction?.RequestStatusDescription}"
                    };

                    _ = events.RaiseTransactionEvent(transaction, CustomEvent.TransactionRejected, message);
                    return BadRequest(new OperationResponse(message, StatusEnum.Error, transaction.PaymentTransactionID));
                }
            }
        }
    }
}