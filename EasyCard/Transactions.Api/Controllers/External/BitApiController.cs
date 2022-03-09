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
using Transactions.Api.Models.Transactions.Enums;
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

                if (bitProcessorConfig?.Settings != null)
                {
                    var bitSettings = bitProcessorConfig.Settings.ToObject<BitTerminalSettings>();
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

                transaction.BitPaymentInitiationId = bitResponse.PaymentInitiationId;
                transaction.BitTransactionSerialId = bitResponse.TransactionSerialId;
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
                    BitPaymentInitiationId = transaction.BitPaymentInitiationId,
                    BitTransactionSerialId = transaction.BitTransactionSerialId,
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
                PaymentRequest dbPaymentRequest = null;
                bool isPaymentIntent = false;

                if (model.PaymentIntentID != null)
                {
                    dbPaymentRequest = EnsureExists(await paymentIntentService.GetPaymentIntent(model.PaymentIntentID.GetValueOrDefault()), "PaymentIntent");

                    isPaymentIntent = true;
                }
                else
                {
                    dbPaymentRequest = EnsureExists(await paymentRequestsService.GetPaymentRequests().FirstOrDefaultAsync(m => m.PaymentRequestID == model.PaymentRequestID));

                    if (dbPaymentRequest.Status == PaymentRequestStatusEnum.Payed || (int)dbPaymentRequest.Status < 0 || dbPaymentRequest.PaymentTransactionID != null)
                    {
                        return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.PaymentRequestStatusIsClosed}", StatusEnum.Error, dbPaymentRequest.PaymentRequestID, httpContextAccessor.TraceIdentifier));
                    }
                }

                var transaction = EnsureExists(await transactionsService.GetTransaction(t => t.PaymentTransactionID == model.PaymentTransactionID));

                Terminal terminal = EnsureExists(await terminalsService.GetTerminal(transaction.TerminalID));

                //already updated
                if (transaction.Status != TransactionStatusEnum.ConfirmedByAggregator && transaction.Status != TransactionStatusEnum.Initial)
                {
                    return new OperationResponse(Shared.Messages.TransactionStatusIsNotValid, StatusEnum.Error, model.PaymentTransactionID);
                }

                var processorRequest = mapper.Map<ProcessorCreateTransactionRequest>(transaction);
                processorRequest.CorrelationId = transaction.CorrelationId ?? GetCorrelationID();

                var bitCaptureResponse = await bitProcessor.CaptureTransaction(processorRequest);

                if (bitCaptureResponse == null || bitCaptureResponse.MessageCode != null)
                {
                    return new OperationResponse(Shared.Messages.BitPaymentFailed, StatusEnum.Error, model.PaymentTransactionID);
                }

                if (!string.IsNullOrEmpty(bitCaptureResponse.SuffixPlasticCardNumber))
                {
                    transaction.CreditCardDetails = new Business.Entities.CreditCardDetails
                    {
                        CardNumber = $"000000000000{bitCaptureResponse.SuffixPlasticCardNumber}",
                    };
                }

                //await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.ConfirmedByProcessor);

                //TODO
                //if (!model.Success)
                //{
                //    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedByProcessor, TransactionFinalizationStatusEnum.Initial, rejectionMessage: model.ResultText/*, rejectionReason: model.ResultCode*/);
                //}
                //else
                //{
                //    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.ConfirmedByProcessor);
                //}

                var terminalAggregator = ValidateExists(
                   terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator),
                   Transactions.Shared.Messages.AggregatorNotDefined);

                var aggregator = aggregatorResolver.GetAggregator(terminalAggregator);

                var aggregatorSettings = aggregatorResolver.GetAggregatorTerminalSettings(terminalAggregator, terminalAggregator.Settings);
                mapper.Map(aggregatorSettings, transaction);

                var bitTransaction = await bitProcessor.GetBitTransaction(model.PaymentInitiationId, model.PaymentTransactionID.ToString(), Guid.NewGuid().ToString(), GetCorrelationID());

                if (aggregator.ShouldBeProcessedByAggregator(transaction.TransactionType, transaction.SpecialTransactionType, transaction.JDealType))
                {
                    //TODO
                    //// reject to clearing house in case of shva error
                    if (!bitTransaction.Success)
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

                                return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}: {aggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier));
                            }
                            else
                            {
                                await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.CanceledByAggregator);

                                return new OperationResponse(Shared.Messages.BitPaymentFailed, StatusEnum.Error, model.PaymentTransactionID);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"{nameof(BitApiController)}.{nameof(Capture)}: Aggregator Cancel Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                            await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.FailedToCancelByAggregator);

                            return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionFinalizationStatusEnum.FailedToCancelByAggregator.ToString(), (ex as IntegrationException)?.Message));
                        }
                    }
                    else  // commit transaction in aggregator (Clearing House or upay)
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
                                // NOTE: In case of failed commit, transaction should not be transmitted to Shva

                                logger.LogError($"{nameof(BitApiController)}.{nameof(Capture)}: Aggregator Commit Transaction request error. TransactionID: {transaction.PaymentTransactionID}");

                                await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToCommitByAggregator, rejectionMessage: commitAggregatorResponse.ErrorMessage, rejectionReason: commitAggregatorResponse.RejectReasonCode);

                                return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}: {commitAggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, commitAggregatorResponse.Errors));
                            }
                            else
                            {
                                await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.Completed, transactionOperationCode: TransactionOperationCodesEnum.CommitedByAggregator);
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
                else
                {
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.Completed);
                }

                if (isPaymentIntent)
                {
                    await paymentIntentService.DeletePaymentIntent(model.PaymentIntentID.GetValueOrDefault());
                }
                else
                {
                    await paymentRequestsService.UpdateEntityWithStatus(dbPaymentRequest, PaymentRequestStatusEnum.Payed, paymentTransactionID: model.PaymentTransactionID, message: Transactions.Shared.Messages.PaymentRequestPaymentSuccessed);
                }

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

                return new OperationResponse(Shared.Messages.TransactionUpdated, StatusEnum.Success, model.PaymentTransactionID);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to capture Transaction for Bit. Transaction id: {model.PaymentTransactionID}");
                return new OperationResponse(ex.Message, StatusEnum.Error);
            }
        }

    }

}