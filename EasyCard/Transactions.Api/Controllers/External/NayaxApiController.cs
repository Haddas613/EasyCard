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
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Validation;
using Shared.Business.Security;
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
using SharedApi = Shared.Api;

namespace Transactions.Api.Controllers.External
{
    [Authorize(AuthenticationSchemes = Extensions.Auth.ApiKeyAuthenticationScheme, Policy = Policy.NayaxAPI)]
    [Route("api/external/nayax")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class NayaxApiController : ApiControllerBase
    {
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly IAggregatorResolver aggregatorResolver;
        private readonly INayaxTransactionsParametersService nayaxTransactionsService;
        private readonly ITransactionsService transactionsService;
        private readonly IMetricsService metrics;
        private readonly ITerminalsService terminalsService;
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
             IOptions<NayaxGlobalSettings> configuration)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.aggregatorResolver = aggregatorResolver;
            this.nayaxTransactionsService = nayaxTransactionsService;
            this.transactionsService = transactionsService;
            this.metrics = metrics;
            this.terminalsService = terminalsService;
            this.logger = logger;
            this.configuration = configuration.Value;
            this.mapper = mapper;
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
                var terminals = terminalsService.GetTerminals();
                bool foundTerminal = false;
                Terminal terminalMakingTransaction = null;
                foreach (var terminal in terminals)
                {
                    var validterminal = terminalsService.GetTerminal(terminal.TerminalID);
                    if (validterminal == null)
                    {
                        continue;
                    }

                    var nayaxIntegrationn = terminal.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.NayaxPinpadProcessorExternalSystemID);
                    if (nayaxIntegrationn == null)
                    {
                        continue;
                    }

                    var devicess = nayaxIntegrationn.Settings.ToObject<NayaxTerminalCollection>();

                    var device = devicess.devices.FirstOrDefault(x => x.TerminalID == model.TerminalDetails.ClientToken);
                    if (device != null)
                    {
                        foundTerminal = true;
                        terminalMakingTransaction = terminal;
                        break;
                    }
                }

                if (!foundTerminal)
                {
                    return new NayaxResult("Couldn't find valid terminal details", false);
                }


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

                // NOTE: this is security assignment
                mapper.Map(terminalMakingTransaction, transaction);

                await transactionsService.CreateEntity(transaction);
                metrics.TrackTransactionEvent(transaction, TransactionOperationCodesEnum.TransactionCreated);
                //var clientCode = client.ClientCode;
                //bool isClearingHouse = TerminalMakingTransaction.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.ClearingHouseExternalSystemID) != null;


                // bool isUpay = TerminalMakingTransaction.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.UpayExternalSystemID) != null;

                var terminalAggregator = ValidateExists(
              terminalMakingTransaction.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator),
              Transactions.Shared.Messages.AggregatorNotDefined);

                var aggregator = aggregatorResolver.GetAggregator(terminalAggregator);

                if (aggregator.ShouldBeProcessedByAggregator(transaction.TransactionType, transaction.SpecialTransactionType, transaction.JDealType))
                {
                    try
                    {
                        var aggregatorRequest = mapper.Map<AggregatorCreateTransactionRequest>(model);
                        aggregatorRequest.TransactionDate = DateTime.Now;
                        //aggregatorRequest.TransactionID
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

                //bool isRavMutav = (client.SapakMotav ?? false) && !String.IsNullOrEmpty(client.SapakMotavNumber); todo
                //string RavMutav = client.SapakMotavNumber;   todo
                string transactionID = null; //for ClearingHouse
                string concurencyToken = null;

                string expDate_YYMM = string.Format("{0}{1}", model.CardExpiry.Substring(2, 2), model.CardExpiry.Substring(0, 2));
                var cardNumber = NayaxHelper.GetCardNumber(model.MaskedPan);
                var cardExmp = model.CardExpiry;
                var last4Digits = cardNumber.Substring(cardNumber.Length - 4, 4);
                var cardBin = cardNumber.Substring(0, 6).Replace('*', '0');
                #region double_transaction
                /*to do block double transactions
                 * bool allowDoubleTransactions = client.AllowDoubleTransactions ?? false;

                 if (!allowDoubleTransactions)
                 {
                     string expdate = "0000";
                     if (!String.IsNullOrEmpty(expDate_YYMM) && expDate_YYMM.Length >= 4)
                         expdate = string.Format("{0}/{1}", expDate_YYMM.Substring(2, 2), expDate_YYMM.Substring(0, 2));

                     int duplicateTransactionsBlockTimeInMin = client.DuplicateTransactionsBlockTimeInMin;

                     var ddr = Common.BL.DealInfo.CheckDoubleDeal(client.ClientID ?? -1, last4Digits, expdate, (double)requestForValidateDeal.TransactionAmount / 100, duplicateTransactionsBlockTimeInMin);
                     if (ddr != null)
                     {
                         #region fill_Billingmodel
                         BillingModel billingModel = new BillingModel();
                         billingModel.CardBin = cardBin;
                         billingModel.Month = requestForValidateDeal.CardExpiry.Substring(0, 2);
                         billingModel.Year = requestForValidateDeal.CardExpiry.Substring(2, 2);
                         billingModel.CardNumber = cardNumber;
                         switch (requestForValidateDeal.CreditTerms)
                         {

                             case CreditTermsEnum.regular:
                                 billingModel.DealType = DealTypeEnum.CREDIT_CARD_REGULAR_CREDIT;
                                 break;
                             case CreditTermsEnum.isracredit_sdif_30:
                                 billingModel.DealType = DealTypeEnum.CREDIT_CARD_PLUS_30;
                                 break;
                             case CreditTermsEnum.immediate:
                                 billingModel.DealType = DealTypeEnum.CREDIT_CARD_INSTANT_BILLING;
                                 break;
                             case CreditTermsEnum.credit:
                                 billingModel.DealType = DealTypeEnum.CREDIT_CARD_CREDITS;
                                 break;
                             case CreditTermsEnum.installments:
                                 billingModel.DealType = DealTypeEnum.CREDIT_CARD_PAYMENTS;
                                 break;
                             default:
                                 break;
                         }

                         billingModel‎.Opt‎ = requestForValidateDeal.TranType == TranTypeEnum.refund ? 51 : 1;
                         billingModel‎.ParamJ‎ = ParamJEnum.ביצוע_עסקה;

                         //billingModel.FirstPay;
                         //		billingModel.OtherPay;
                         billingModel.Sum = (double)requestForValidateDeal.TransactionAmount / 100;
                         switch (requestForValidateDeal.OriginalCurrency)
                         {
                             case Common.Enums.CommonEnums.CurrencyEnumISO_Code.EUR:
                                 billingModel.MType = MTypeEnum.EURO;
                                 break;
                             case Common.Enums.CommonEnums.CurrencyEnumISO_Code.USD:
                                 billingModel.MType = MTypeEnum.DOLAR;
                                 break;
                             case Common.Enums.CommonEnums.CurrencyEnumISO_Code.ILS:
                                 billingModel.MType = MTypeEnum.SHEKEL;
                                 break;

                         }

                         billingModel.DealSource = DealSourceEnum.RestEmv;
                         #endregion
                         tblCardRejected card = Common.Helpers.DealHelper.ParseTblCardPreInserted(client.ClientID ?? -1, ddr.ErrorMessage, "-1", "", false, billingModel, "", null, null);
                         Common.BL.DealInfo.AddTotblCardRejected(card);
                         //DeleteFile(String.Format("{0}\\{1}_{2}", System.Configuration.ConfigurationManager.AppSettings["DealsInfoDir"], clientID, dealInfoKeyFile.ToString()));
                         ActivityLogger.Write(string.Format("ValidateDeal result Approval:{0}, Result:{1}", false, ddr.ErrorMessage), client.ClientID ?? -1);
                         return new NayaxResult(ddr.ErrorMessage, false);
                     }
                 }*/
                #endregion



                //  EMVRestTransaction NayaxTran = new EMVRestTransaction();
                string vuid = model.Vuid;
                if (string.IsNullOrEmpty(model.Vuid))
                {
                    // int tranId = Common.BL.Modularity.GetCountTransactionPerTerminalIDsPerClient(_clientID, requestForValidateDeal.TerminalDetails.ClientToken);
                    vuid = string.Format("{0}_{1}", model.TerminalDetails.ClientToken, Guid.NewGuid().ToString());
                }
                //todo save vuid in transaction
                // setValuesToEMVRestTran(requestForValidateDeal, _clientID, transactionID, concurencyToken, NayaxTran, vuid, RavMutav);
                //Common.BL.DealInfo.SaveEMVRestAfterValidate(NayaxTran);
                // string SysTranceNumber = PinPadModularityHelper.GetSysTranceNumber(_clientID);
                return new NayaxResult(string.Empty, true, vuid, null, GetCorrelationID(), string.Empty /*todo RavMutav*/);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to validate Transaction for PAX deal. Vuid: {model.Vuid}");
                return new NayaxResult(ex.Message, false /*ResultEnum.ServerError, false*/);
            }
        }

        [HttpPost]
        [Route("v1/update")]

    }
