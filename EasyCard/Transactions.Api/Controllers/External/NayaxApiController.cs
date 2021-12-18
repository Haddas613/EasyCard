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
using Transactions.Api.Models.Transactions.Enums;
using Transactions.Api.Services;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Transactions.Shared.Enums;
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
        private readonly IMetricsService metrics;
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
             IMetricsService metrics,
             IHttpContextAccessorWrapper httpContextAccessor,
             IOptions<NayaxGlobalSettings> configuration,
             IPinPadDevicesService pinPadDevicesService)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.aggregatorResolver = aggregatorResolver;
            this.nayaxTransactionsService = nayaxTransactionsService;
            this.transactionsService = transactionService;
            this.metrics = metrics;
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
            try
            {
                await nayaxTransactionsService.CreateEntity(new Business.Entities.NayaxTransactionsParameters
                {
                    TranRecord = model.TranRecord,
                    PinPadTransactionID = model.Vuid
                });

                return new NayaxUpdateTranRecordResponse
                {
                    Status = "0"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to update TransactionRecord for PAX deal. Vuid: {model.Vuid} Uid: {model.Uid} ");

                return BadRequest(new NayaxUpdateTranRecordResponse
                {
                    StatusCode = 14,
                    ErrorMsg = string.Format("Failed to update TransactionRecord for PAX deal. Vuid: {0} Uid: {1} ", model.Vuid, model.Uid),
                    Status = "error",
                    CorrelationID = GetCorrelationID()
                });
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

                /*TODO validation for sum
         /// <summary>
         /// In Agorot, with leading zeros
         /// </summary>
         public int FirstPaymentAmount { get; set; }
         /// <summary>
         /// In Agorot, with leading zeros
         /// </summary>
         public int NextPaymentAmount { get; set; }
                  * */

                var transaction = mapper.Map<PaymentTransaction>(model);

                string vuid = model.Vuid;
                if (string.IsNullOrEmpty(model.Vuid))
                {
                    vuid = string.Format("{0}_{1}", model.TerminalDetails.ClientToken, Guid.NewGuid().ToString());
                }

                transaction.PinPadTransactionDetails.PinPadTransactionID = vuid;
                transaction.PinPadTransactionDetails.PinPadCorrelationID = GetCorrelationID();
                transaction.CardPresence = getCardPresence(model.EntryMode);
                transaction.PinPadDeviceID = model.TerminalDetails.ClientToken;
                transaction.DocumentOrigin = DocumentOriginEnum.Device;
                // NOTE: this is security assignment
                mapper.Map(terminalMakingTransaction, transaction);

                transaction.VATRate = terminalMakingTransaction.Settings.VATRate.GetValueOrDefault(0);
                transaction.DealDetails.UpdateDealDetails(null, terminalMakingTransaction.Settings, transaction, transaction.CreditCardDetails);
                transaction.Calculate();

                await transactionsService.CreateEntity(transaction);
                metrics.TrackTransactionEvent(transaction, TransactionOperationCodesEnum.TransactionCreated);

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

                string sysTranceNumber = getSysTranceNumber(terminalMakingTransaction);
                return new NayaxResult(string.Empty, true, transaction.PinPadTransactionDetails.PinPadTransactionID, sysTranceNumber, transaction.PinPadTransactionDetails.PinPadCorrelationID, terminalMakingTransaction.Settings?.RavMutavNumber);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to validate Transaction for PAX deal. Vuid: {model.Vuid}");
                return new NayaxResult(ex.Message, false /*ResultEnum.ServerError, false*/);
            }
        }

        private string getSysTranceNumber(Terminal terminalMakingTransaction)
        {
            var terminalProcessor = terminalMakingTransaction.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Processor);
            Shva.ShvaTerminalSettings terminalSettings = terminalProcessor.Settings.ToObject<Shva.ShvaTerminalSettings>();
            ShvaTransactionDetails lastDealShvaDetails = transactionsService.GetTransactions().Where(x => x.ShvaTransactionDetails.ShvaTerminalID == terminalSettings.MerchantNumber && x.ShvaTransactionDetails != null && x.ShvaTransactionDetails.ShvaShovarNumber != null).OrderByDescending(d => d.TransactionDate).Select(d => d.ShvaTransactionDetails).FirstOrDefaultAsync().Result;
            var sysTranceNumber = Nayax.Converters.EMVDealHelper.GetFilNSeq(lastDealShvaDetails.ShvaShovarNumber, lastDealShvaDetails.TransmissionDate);
            return sysTranceNumber;
        }
        private CardPresenceEnum getCardPresence(EntryModeEnum entryMode)
        {
            return entryMode.IsIn(EntryModeEnum.CellularPhoneNum, EntryModeEnum.PhoneTran)? CardPresenceEnum.CardNotPresent : CardPresenceEnum.Regular;
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
                //todo update transaction with all details from Nayax
                transaction.ShvaTransactionDetails.ShvaDealID = model.Uid;
                transaction.ShvaTransactionDetails.Solek = Transactions.Api.Extensions.TransactionHelpers.GetTransactionSolek(model.Aquirer);
                transaction.CreditCardDetails.CardBrand = model.Brand.ToString();
                transaction.CreditCardDetails.CardVendor = ((int)model.Issuer).ToString();
                transaction.ShvaTransactionDetails.ShvaAuthNum = model.Issuer_Auth_Num;
                transaction.ShvaTransactionDetails.ShvaShovarNumber = model.DealNumber;
                transaction.PinPadTransactionDetails.PinPadUpdateReceiptNumber = updateReceiptNumber.ToString();
                if (!model.Success)
                {
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedByProcessor, TransactionFinalizationStatusEnum.Initial, rejectionMessage: model.ResultText/*, rejectionReason: model.ResultCode*/);

                }
                else
                {
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.ConfirmedByProcessor);
                }

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

                                return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}: {aggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier));
                            }
                            else
                            {
                                await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.CanceledByAggregator);

                                return new NayaxResult()
                                {
                                    CorrelationID = model.CorrelationID,
                                    Approval = true,
                                    ResultText = TransactionFinalizationStatusEnum.CanceledByAggregator.ToString(),
                                    UpdateReceiptNumber = updateReceiptNumber.ToString()
                                };
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Aggregator Cancel Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

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
                                await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.ConfirmedByAggregator, transactionOperationCode: TransactionOperationCodesEnum.CommitedByAggregator);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Aggregator Commit Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                            await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToCommitByAggregator, rejectionReason: RejectionReasonEnum.Unknown, rejectionMessage: ex.Message);

                            return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionStatusEnum.FailedToCommitByAggregator.ToString(), (ex as IntegrationException)?.Message));
                        }
                    }
                }

                return new NayaxResult(string.Empty, true, transaction.PinPadTransactionDetails.PinPadTransactionID, null, transaction.PinPadTransactionDetails.PinPadCorrelationID, string.Empty /*todo RavMutav*/, updateReceiptNumber.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to update Transaction for PAX deal. Vuid: {model.Vuid}");
                return new NayaxResult(ex.Message, false /*ResultEnum.ServerError, false*/);
            }
        }

    }

}