using AutoMapper;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nayax;
using Nayax.Configuration;
using Nayax.Models;
using Shared.Api;
using Shared.Api.Attributes;
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
using Transactions.Api.Models.Transactions;
using Transactions.Api.Services;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Transactions.Shared.Enums;
using Nayax.Converters;
using Shared.Helpers.Events;
using SharedApi = Shared.Api;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Controllers.External
{
    [Authorize(AuthenticationSchemes = Extensions.Auth.ApiKeyAuthenticationScheme, Policy = Policy.NayaxAPI)]
    [Route("api/external/nayax")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [PascalCaseOutput]
    public class NayaxApiController : ApiControllerBase
    {
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly IAggregatorResolver aggregatorResolver;
        private readonly INayaxTransactionsParametersService nayaxTransactionsService;
        private readonly ITransactionsService transactionsService;
        private readonly IEventsService events;
        private readonly ITerminalsService terminalsService;
        private readonly IPinPadDevicesService pinPadDevicesService;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly NayaxGlobalSettings configuration;

        public NayaxApiController(
             IAggregatorResolver aggregatorResolver,
             INayaxTransactionsParametersService nayaxTransactionsService,
             ITransactionsService transactionService,
             ITerminalsService terminalsService,
             IMapper mapper,
             ILogger<TransactionsApiController> logger,
             IEventsService events,
             IHttpContextAccessorWrapper httpContextAccessor,
             IOptions<NayaxGlobalSettings> configuration,
             IPinPadDevicesService pinPadDevicesService)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.aggregatorResolver = aggregatorResolver;
            this.nayaxTransactionsService = nayaxTransactionsService;
            this.transactionsService = transactionService;
            this.events = events;
            this.terminalsService = terminalsService;
            this.logger = logger;
            this.configuration = configuration.Value;
            this.mapper = mapper;
            this.pinPadDevicesService = pinPadDevicesService;
        }

        [HttpPost]
        [Route("v1/tranRecord")]
        public async Task<ActionResult<NayaxUpdateTranRecordResponse>> UpdateTranRecord([FromBody] NayaxUpdateTranRecordRequest model)
        {
            // TODO: check if entity already exist
            try
            {
                Guid tranRecordReceiptNumber = Guid.NewGuid();
                await nayaxTransactionsService.CreateEntity(new Business.Entities.NayaxTransactionsParameters
                {
                    TranRecord = model.TranRecord,
                    PinPadTransactionID = model.Vuid,
                    PinPadTranRecordReceiptNumber = tranRecordReceiptNumber.ToString()
                });

                return new NayaxUpdateTranRecordResponse
                {
                    Status = "0",
                    ReceiptNumber = tranRecordReceiptNumber.ToString()
                };
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Failed to update TransactionRecord for PAX deal. Vuid: {model.Vuid} Uid: {model.Uid} ");

                return new NayaxUpdateTranRecordResponse
                {
                    StatusCode = 14,
                    Status = "error",
                    ErrorMsg = $"Failed to update TransactionRecord for PAX deal. Vuid: {model.Vuid} Uid: {model.Uid} "
                };
            }
        }

        [HttpPost]
        [ValidateModelState]
        [Route("v1/validate")]
        public async Task<ActionResult<NayaxResult>> Validate([FromBody] NayaxValidateRequest model)
        {
            try
            {
                var pinPadDevice = await pinPadDevicesService.GetDevice(model.TerminalDetails.ClientToken);

                if (pinPadDevice is null)
                {
                    return new NayaxResult("Couldn't find valid terminal details", false);
                }

                Terminal terminalMakingTransaction = EnsureExists(await terminalsService.GetTerminal(pinPadDevice.TerminalID.Value));

                var terminalAggregator = ValidateExists(
                  terminalMakingTransaction.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator),
                  Transactions.Shared.Messages.AggregatorNotDefined);

                var transaction = mapper.Map<PaymentTransaction>(model);

                string vuid = model.Vuid;
                if (string.IsNullOrEmpty(model.Vuid))
                {
                    vuid = string.Format("{0}_{1}", model.TerminalDetails.ClientToken, Guid.NewGuid().ToString());
                }

                SetCardDetails(model, transaction);
                transaction.PinPadTransactionDetails.PinPadTransactionID = vuid;
                transaction.PinPadTransactionDetails.PinPadCorrelationID = GetCorrelationID();
                transaction.CardPresence = GetCardPresence(model.EntryMode);
                transaction.PinPadDeviceID = model.TerminalDetails.ClientToken;

                if (transaction.ShvaTransactionDetails == null)
                {
                    transaction.ShvaTransactionDetails = new ShvaTransactionDetails();
                }

                transaction.ShvaTransactionDetails.ShvaTerminalID = GetShvaTerminal(terminalMakingTransaction);
                transaction.DocumentOrigin = DocumentOriginEnum.Device;
                mapper.Map(terminalMakingTransaction, transaction);

                transaction.VATRate = terminalMakingTransaction.Settings.VATRate.GetValueOrDefault(0);
                transaction.DealDetails.UpdateDealDetails(null, terminalMakingTransaction.Settings, transaction, transaction.CreditCardDetails);
                transaction.Calculate();

                await transactionsService.CreateEntity(transaction);

                var aggregator = aggregatorResolver.GetAggregator(terminalAggregator);

                if (aggregator.ShouldBeProcessedByAggregator(transaction.TransactionType, transaction.SpecialTransactionType, transaction.JDealType))
                {
                    try
                    {
                        var aggregatorRequest = mapper.Map<AggregatorCreateTransactionRequest>(transaction);
                        aggregatorRequest.CreditCardDetails = new SharedIntegration.Models.CreditCardDetails();
                        mapper.Map(model, aggregatorRequest.CreditCardDetails);

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

                string sysTranceNumber = GetSysTranceNumber(terminalMakingTransaction);
                return new NayaxResult(string.Empty, true, transaction.PinPadTransactionDetails.PinPadTransactionID, sysTranceNumber, transaction.PinPadTransactionDetails.PinPadCorrelationID, terminalMakingTransaction.Settings?.RavMutavNumber);


            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to validate Transaction for PAX deal. Vuid: {model.Vuid}");
                return new NayaxResult($"Failed to validate Transaction for PAX deal. Vuid: {model.Vuid}" + ex.Message, false /*ResultEnum.ServerError, false*/);
            }
        }

        [HttpPost]
        [ValidateModelState]
        [Route("v1/update")]
        public async Task<ActionResult<NayaxResult>> Update([FromBody] NayaxUpdateRequest model)
        {
            try
            {
                var pinPadDevice = await pinPadDevicesService.GetDevice(model.TerminalDetails.ClientToken);

                if (pinPadDevice is null)
                {
                    return new NayaxResult("Couldn't find valid terminal details", false);
                }

                Terminal terminalMakingTransaction = EnsureExists(await terminalsService.GetTerminal(pinPadDevice.TerminalID.Value));

                var transaction = EnsureExists(await transactionsService.GetTransaction(t => t.PinPadTransactionDetails.PinPadTransactionID == model.Vuid && t.PinPadTransactionDetails.PinPadCorrelationID == model.CorrelationID));

                //already updated
                if (transaction.ShvaTransactionDetails?.ShvaDealID == model.Uid)
                {
                    return new NayaxResult { Vuid = transaction.PinPadTransactionDetails.PinPadTransactionID, Approval = true, ResultText = "Success", CorrelationID = model.CorrelationID, UpdateReceiptNumber = transaction.PinPadTransactionDetails.PinPadUpdateReceiptNumber };
                }

                Guid updateReceiptNumber = Guid.NewGuid();
                transaction.ShvaTransactionDetails.ShvaDealID = model.Uid;
                transaction.ShvaTransactionDetails.Solek = model.Aquirer.GetTransactionSolek();
                transaction.CreditCardDetails.CardBrand = model.Brand.ToString();
                transaction.CreditCardDetails.CardVendor = ((int)model.Issuer).ToString();
                transaction.ShvaTransactionDetails.ShvaAuthNum = model.Issuer_Auth_Num;
                transaction.ShvaTransactionDetails.ShvaShovarNumber = model.DealNumber;
                transaction.ProcessorResultCode = model.ResultCode;
                transaction.PinPadTransactionDetails.PinPadUpdateReceiptNumber = updateReceiptNumber.ToString();
                if (!model.Success)
                {
                    transaction.RejectionMessage = model.ResultText;
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedByProcessor, TransactionFinalizationStatusEnum.Initial, rejectionMessage: model.ResultText/*, rejectionReason: model.ResultCode*/);
                }
                else
                {
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.ConfirmedByProcessor);
                }

                var processResult = await ProcessUpdateTransaction(model, transaction, terminalMakingTransaction, updateReceiptNumber);

                if (processResult.Approval)
                {
                    _ = events.RaiseTransactionEvent(transaction, CustomEvent.TransactionCreated);
                }
                else
                {
                    _ = events.RaiseTransactionEvent(transaction, CustomEvent.TransactionRejected);
                }

                return processResult;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to update Transaction for PAX deal. Vuid: {model.Vuid}");

                var transaction = await transactionsService.GetTransaction(t => t.PinPadTransactionDetails.PinPadTransactionID == model.Vuid && t.PinPadTransactionDetails.PinPadCorrelationID == model.CorrelationID);

                if (transaction != null)
                {
                    _ = events.RaiseTransactionEvent(transaction, CustomEvent.TransactionRejected);
                }

                return new NayaxResult(ex.Message, false /*ResultEnum.ServerError, false*/);
            }
        }

        private async Task<NayaxResult> ProcessUpdateTransaction(NayaxUpdateRequest model, PaymentTransaction transaction, Terminal terminalMakingTransaction, Guid updateReceiptNumber)
        {
            var terminalAggregator = ValidateExists(
                   terminalMakingTransaction.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator),
                   Transactions.Shared.Messages.AggregatorNotDefined);

            var aggregator = aggregatorResolver.GetAggregator(terminalAggregator);

            var aggregatorSettings = aggregatorResolver.GetAggregatorTerminalSettings(terminalAggregator, terminalAggregator.Settings);
            mapper.Map(aggregatorSettings, transaction);

            if (aggregator.ShouldBeProcessedByAggregator(transaction.TransactionType, transaction.SpecialTransactionType, transaction.JDealType))
            {
                // reject to clearing house in case of shva error
                if (!model.Success)
                {
                    try
                    {
                        var aggregatorRequest = mapper.Map<AggregatorCancelTransactionRequest>(transaction);
                        aggregatorRequest.AggregatorSettings = aggregatorSettings;
                        aggregatorRequest.RejectionReason = TransactionStatusEnum.FailedToConfirmByProcesor.ToString();

                        var aggregatorResponse = await aggregator.CancelTransaction(aggregatorRequest);
                        mapper.Map(aggregatorResponse, transaction);

                        if (!aggregatorResponse.Success)
                        {
                            logger.LogError($"Aggregator Cancel Transaction request error. TransactionID: {transaction.PaymentTransactionID}");

                            await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.FailedToCancelByAggregator);

                            return new NayaxResult()
                            {
                                CorrelationID = model.CorrelationID,
                                Approval = false,
                                ResultText = $"{Shared.Messages.FailedToProcessTransaction}: {aggregatorResponse.ErrorMessage}",
                                UpdateReceiptNumber = updateReceiptNumber.ToString()
                            };

                            //return BadRequest(new OperationResponse(, StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier));
                        }
                        else
                        {
                            await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.CanceledByAggregator);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Aggregator Cancel Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                        await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.FailedToCancelByAggregator);

                        return new NayaxResult()
                        {
                            CorrelationID = model.CorrelationID,
                            Approval = false,
                            ResultText = Shared.Messages.FailedToCancelByAggregator,
                            UpdateReceiptNumber = updateReceiptNumber.ToString()
                        };

                        //return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionFinalizationStatusEnum.FailedToCancelByAggregator.ToString(), (ex as IntegrationException)?.Message));
                    }
                }
                else // commit transaction in aggregator (Clearing House or upay)
                {
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

                            return new NayaxResult()
                            {
                                CorrelationID = model.CorrelationID,
                                Approval = false,
                                ResultText = Shared.Messages.FailedToCommitByAggregator,
                                UpdateReceiptNumber = updateReceiptNumber.ToString()
                            };

                            //return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}: {commitAggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, commitAggregatorResponse.Errors));
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

                        return new NayaxResult()
                        {
                            CorrelationID = model.CorrelationID,
                            Approval = false,
                            ResultText = Shared.Messages.FailedToCommitByAggregator,
                            UpdateReceiptNumber = updateReceiptNumber.ToString()
                        };

                        //return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionStatusEnum.FailedToCommitByAggregator.ToString(), (ex as IntegrationException)?.Message));
                    }
                }
            }
            else
            {
                //If aggregator is not required transaction is eligible for transmission

                if (model.Success)
                {
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.AwaitingForTransmission);
                }
            }

            return new NayaxResult(Shared.Messages.TransactionCreated, true, transaction.PinPadTransactionDetails.PinPadTransactionID, null, transaction.PinPadTransactionDetails.PinPadCorrelationID, terminalMakingTransaction.Settings?.RavMutavNumber, updateReceiptNumber.ToString());
        }

        // TODO: move to Nayax.Converters.MetadataConvertor
        private static void SetCardDetails(NayaxValidateRequest model, PaymentTransaction transaction)
        {
            var cardNumber = model.MaskedPan.GetCardNumber(); // TODO: this works wrong
            var cardExmp = model.CardExpiry;
            int month = -1;
            int.TryParse(cardExmp.Substring(0, 2), out month);
            int year = -1;
            int.TryParse(cardExmp.Substring(2, 2), out year);

            var cardBin = cardNumber.Substring(0, 6).Replace('*', '0');
            transaction.CreditCardDetails = new Business.Entities.CreditCardDetails { CardExpiration = new CardExpiration { Month = month, Year = year }, CardBin = cardBin, CardNumber = cardNumber };
        }

        // TODO: review in perspective of sql transaction
        private static string GetShvaTerminal(Terminal terminalMakingTransaction)
        {
            var terminalProcessor = terminalMakingTransaction.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Processor);
            Shva.ShvaTerminalSettings terminalSettings = terminalProcessor.Settings.ToObject<Shva.ShvaTerminalSettings>();
            return terminalSettings.MerchantNumber;
        }

        // TODO: review in perspective of sql transaction
        private string GetSysTranceNumber(Terminal terminalMakingTransaction)
        {
            var terminalProcessor = terminalMakingTransaction.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Processor);
            Shva.ShvaTerminalSettings terminalSettings = terminalProcessor.Settings.ToObject<Shva.ShvaTerminalSettings>();
            ShvaTransactionDetails lastDealShvaDetails = transactionsService.GetTransactions().Where(x => x.ShvaTransactionDetails.ShvaTerminalID == terminalSettings.MerchantNumber && x.ShvaTransactionDetails != null && x.ShvaTransactionDetails.ShvaShovarNumber != null).OrderByDescending(d => d.TransactionDate).Select(d => d.ShvaTransactionDetails).FirstOrDefaultAsync().Result;
            var sysTranceNumber = lastDealShvaDetails == null ? "{1;1}" : Nayax.Converters.EMVDealHelper.GetFilNSeq(lastDealShvaDetails.ShvaShovarNumber, lastDealShvaDetails.TransmissionDate);
            return sysTranceNumber;
        }

        // TODO: move to Nayax.Converters.MetadataConvertor
        private CardPresenceEnum GetCardPresence(EntryModeEnum entryMode)
        {
            return entryMode.IsIn(EntryModeEnum.CellularPhoneNum, EntryModeEnum.PhoneTran) ? CardPresenceEnum.CardNotPresent : CardPresenceEnum.Regular;
        }
    }
}