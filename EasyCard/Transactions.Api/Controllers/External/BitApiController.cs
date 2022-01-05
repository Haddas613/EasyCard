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
    [PascalCaseOutput]
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
             IOptions<ApiSettings> apiSettings)
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
                processorRequest.RedirectURL = $"{apiSettings.CheckoutPortalUrl}/bit";

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
        [Route("v1/capture")]
        public async Task<ActionResult<OperationResponse>> Capture([FromBody] Models.External.Bit.CaptureBitTransactionRequest model)
        {
            try
            {
                var transaction = EnsureExists(await transactionsService.GetTransaction(t => t.PaymentTransactionID == model.PaymentTransactionID));

                Terminal terminal = EnsureExists(await terminalsService.GetTerminal(transaction.TerminalID));

                //already updated
                if (transaction.Status != TransactionStatusEnum.ConfirmedByAggregator)
                {
                    return new OperationResponse(Shared.Messages.TransactionStatusIsNotValid, StatusEnum.Error, model.PaymentTransactionID);
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

                if (aggregator.ShouldBeProcessedByAggregator(transaction.TransactionType, transaction.SpecialTransactionType, transaction.JDealType))
                {
                    //TODO
                    //// reject to clearing house in case of shva error
                    //if (!model.Success)
                    //{
                    //    try
                    //    {
                    //        var aggregatorRequest = mapper.Map<AggregatorCancelTransactionRequest>(transaction);
                    //        aggregatorRequest.AggregatorSettings = aggregatorSettings;
                    //        aggregatorRequest.RejectionReason = TransactionStatusEnum.FailedToConfirmByProcesor.ToString();

                    //        var aggregatorResponse = await aggregator.CancelTransaction(aggregatorRequest);
                    //        mapper.Map(aggregatorResponse, transaction);

                    //        if (!aggregatorResponse.Success)
                    //        {
                    //            logger.LogError($"Aggregator Cancel Transaction request error. TransactionID: {transaction.PaymentTransactionID}");

                    //            await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.FailedToCancelByAggregator);

                    //            return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}: {aggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier));
                    //        }
                    //        else
                    //        {
                    //            await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.CanceledByAggregator);

                    //            return new NayaxResult()
                    //            {
                    //                CorrelationID = model.CorrelationID,
                    //                Approval = true,
                    //                ResultText = TransactionFinalizationStatusEnum.CanceledByAggregator.ToString(),
                    //                UpdateReceiptNumber = updateReceiptNumber.ToString()
                    //            };
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        logger.LogError(ex, $"Aggregator Cancel Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                    //        await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.FailedToCancelByAggregator);

                    //        return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionFinalizationStatusEnum.FailedToCancelByAggregator.ToString(), (ex as IntegrationException)?.Message));
                    //    }
                    //}
                    //else  // commit transaction in aggregator (Clearing House or upay)
                    //{
                        try
                        {
                            var commitAggregatorRequest = mapper.Map<AggregatorCommitTransactionRequest>(transaction);

                            commitAggregatorRequest.AggregatorSettings = aggregatorSettings;

                            var commitAggregatorResponse = await aggregator.CommitTransaction(commitAggregatorRequest);
                            mapper.Map(commitAggregatorResponse, transaction);

                            if (!commitAggregatorResponse.Success)
                            {
                                // NOTE: In case of failed commit, transaction should not be transmitted to Shva

                                logger.LogError($"Aggregator Commit Transaction request error. TransactionID: {transaction.PaymentTransactionID}");

                                await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToCommitByAggregator, rejectionMessage: commitAggregatorResponse.ErrorMessage, rejectionReason: commitAggregatorResponse.RejectReasonCode);

                                return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}: {commitAggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, commitAggregatorResponse.Errors));
                            }
                            else
                            {
                                await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.AwaitingForTransmission, transactionOperationCode: TransactionOperationCodesEnum.CommitedByAggregator);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Aggregator Commit Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                            await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToCommitByAggregator, rejectionReason: RejectionReasonEnum.Unknown, rejectionMessage: ex.Message);

                            return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionStatusEnum.FailedToCommitByAggregator.ToString(), (ex as IntegrationException)?.Message));
                        }
                    //}
                }
                else
                {
                    //If aggregator is not required transaction is eligible for transmission
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.AwaitingForTransmission);
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