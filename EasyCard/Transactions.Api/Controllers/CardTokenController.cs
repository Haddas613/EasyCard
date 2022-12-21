using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Security.KeyVault.Secrets;
using Merchants.Business.Entities.Billing;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Api.Validation;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.KeyValueStorage;
using Shared.Helpers.Security;
using Shared.Integration.Exceptions;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using Transactions.Api.Extensions.Filtering;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Services;
using Transactions.Api.Validation;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Transactions.Shared;
using Transactions.Shared.Enums;
using Z.EntityFramework.Plus;
using SharedHelpers = Shared.Helpers;

namespace Transactions.Api.Controllers
{
    [Route("api/cardtokens")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
    [ApiController]
    public class CardTokenController : ApiControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly ICreditCardTokenService creditCardTokenService;
        private readonly IMapper mapper;
        private readonly IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage;
        private readonly ApplicationSettings appSettings;
        private readonly ILogger logger;
        private readonly IProcessorResolver processorResolver;
        private readonly IConsumersService consumersService;
        private readonly ITerminalsService terminalsService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ISystemSettingsService systemSettingsService;
        private readonly IBillingDealService billingDealService;

        public CardTokenController(
            ITransactionsService transactionsService,
            ICreditCardTokenService creditCardTokenService,
            IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage,
            IMapper mapper,
            ITerminalsService terminalsService,
            IOptions<ApplicationSettings> appSettings,
            ILogger<CardTokenController> logger,
            IProcessorResolver processorResolver,
            IConsumersService consumersService,
            IHttpContextAccessorWrapper httpContextAccessor,
            ISystemSettingsService systemSettingsService,
            IBillingDealService billingDealService)
        {
            this.transactionsService = transactionsService;
            this.creditCardTokenService = creditCardTokenService;
            this.keyValueStorage = keyValueStorage;
            this.mapper = mapper;
            this.consumersService = consumersService;
            this.terminalsService = terminalsService;
            this.appSettings = appSettings.Value;
            this.logger = logger;
            this.processorResolver = processorResolver;
            this.httpContextAccessor = httpContextAccessor;
            this.systemSettingsService = systemSettingsService;
            this.billingDealService = billingDealService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = (httpContextAccessor.GetUser().IsAdmin() ? typeof(CreditCardTokenSummaryAdmin) : typeof(CreditCardTokenSummary))
                    .GetObjectMeta(CreditCardSummaryResource.ResourceManager, CurrentCulture)
            };
        }

        /// <summary>
        /// Create token for credit card details
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateModelState]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<OperationResponse>> CreateToken([FromBody] TokenRequest model)
        {
            var terminal = await GetTerminal(model.TerminalID);

            var tokenResponse = await CreateTokenInternal(terminal, model);

            var tokenResponseOperation = tokenResponse.GetOperationResponse();

            if (!(tokenResponseOperation?.Status == StatusEnum.Success))
            {
                return tokenResponse;
            }
            else
            {
                return CreatedAtAction(nameof(CreateToken), tokenResponseOperation);
            }
        }

        /// <summary>
        /// Extend card expiration
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("extendExpiration")]
        [HttpPost]
        [ValidateModelState]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperationResponse))]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<OperationResponse>> ExtendExpiration([FromBody] ExtendTokenRequest model)
        {
            var query = creditCardTokenService.GetTokens();

            if (User.IsTerminal())
            {
                var terminal = EnsureExists(await terminalsService.GetTerminal((User.GetTerminalID()?.FirstOrDefault()).GetValueOrDefault()));

                if (terminal.Settings.SharedCreditCardTokens == true)
                {
                    query = creditCardTokenService.GetTokensShared(terminal.TerminalID);
                }
            }

            var token = EnsureExists(await query.FirstOrDefaultAsync(t => t.CreditCardTokenID == model.CreditCardTokenID));
            token.CardExpirationBeforeExtended = token.CardExpiration;
            token.CardExpiration = model.CardExpiration;
            token.Extended = DateTime.UtcNow;

            try
            {
                var kvtoken = EnsureExists(await keyValueStorage.Get(model.CreditCardTokenID.ToString()), "CreditCardToken");
                kvtoken.CardExpiration = model.CardExpiration;

                await keyValueStorage.Save(model.CreditCardTokenID.ToString(), JsonConvert.SerializeObject(kvtoken));
            }
            catch (Exception e)
            {
                logger.LogError(e, $"{nameof(DeleteToken)}: Error while extending token from keyvalue storage. Message: {e.Message}");

                return BadRequest(new OperationResponse($"Error while extending token from keyvalue storage", StatusEnum.Error));
            }

            using (var dbTransaction = creditCardTokenService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
            {
                try
                {
                    await creditCardTokenService.UpdateEntity(token, dbTransaction);

                    // TODO: move to event handler
                    foreach (var billing in await billingDealService.GetBillingDeals().Filter(new Models.Billing.BillingDealsFilter { CreditCardTokenID = token.CreditCardTokenID }).ToListAsync())
                    {
                        // TODO
                        billing.ExtendToken(model.CardExpiration);

                        await billingDealService.UpdateEntityWithHistory(billing, Messages.TokenExtended, BillingDealOperationCodesEnum.CreditCardTokenExtended, dbTransaction);
                    }

                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"{nameof(DeleteToken)}: Error while extending token and updating related data: {ex.Message}");
                    dbTransaction.Rollback();

                    return BadRequest(new OperationResponse($"Error while extending token and updating related data", StatusEnum.Error));
                }
            }

            return Ok(new OperationResponse(Messages.TokenExtended, StatusEnum.Success, model.CreditCardTokenID));
        }

        /// <summary>
        /// Get tokens by filters
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<SummariesResponse<CreditCardTokenSummary>>> GetTokens([FromQuery] CreditCardTokenFilter filter)
        {
            var query = creditCardTokenService.GetTokens().AsNoTracking().Filter(filter);
            var numberOfRecordsFuture = query.DeferredCount().FutureValue();

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                query = query.OrderByDynamic(filter.SortBy ?? nameof(CreditCardTokenDetails.CreditCardTokenID), filter.SortDesc);

                if (httpContextAccessor.GetUser().IsAdmin())
                {
                    var response = new SummariesResponse<CreditCardTokenSummaryAdmin>();
                    var summary = await mapper.ProjectTo<CreditCardTokenSummaryAdmin>(query.ApplyPagination(filter)).Future().ToListAsync();

                    var terminalsId = summary.Where(t => t.TerminalID != null).Select(t => t.TerminalID).Distinct();
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
                    if (User.IsTerminal() || filter.TerminalID.HasValue)
                    {
                        var terminal = EnsureExists(await terminalsService.GetTerminal(filter.TerminalID ?? (User.GetTerminalID()?.FirstOrDefault()).GetValueOrDefault()));

                        if (terminal.Settings.SharedCreditCardTokens == true)
                        {
                            filter.TerminalID = null;
                            query = creditCardTokenService.GetTokensShared(terminal.TerminalID).AsNoTracking().Filter(filter);
                            numberOfRecordsFuture = query.DeferredCount().FutureValue();
                        }
                    }

                    var response = new SummariesResponse<CreditCardTokenSummary>
                    {
                        Data = await mapper.ProjectTo<CreditCardTokenSummary>(query.ApplyPagination(filter)).Future().ToListAsync(),
                        NumberOfRecords = numberOfRecordsFuture.Value
                    };

                    return Ok(response);
                }
            }
        }

        /// <summary>
        /// Delete credit card token
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{key}")]
        public async Task<ActionResult<OperationResponse>> DeleteToken(string key)
        {
            var guid = new Guid(key);
            var query = creditCardTokenService.GetTokens();

            if (User.IsTerminal())
            {
                var terminal = EnsureExists(await terminalsService.GetTerminal((User.GetTerminalID()?.FirstOrDefault()).GetValueOrDefault()));

                if (terminal.Settings.SharedCreditCardTokens == true)
                {
                    query = creditCardTokenService.GetTokensShared(terminal.TerminalID);
                }
            }

            var token = EnsureExists(await query.FirstOrDefaultAsync(t => t.CreditCardTokenID == guid));

            try
            {
                await keyValueStorage.Delete(key);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"{nameof(DeleteToken)}: Error while deleting token from keyvalue storage. Message: {e.Message}");
            }

            using (var dbTransaction = creditCardTokenService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
            {
                try
                {
                    token.Active = false;
                    await creditCardTokenService.UpdateEntity(token, dbTransaction);

                    // TODO: move to event handler
                    foreach (var billing in await billingDealService.GetBillingDeals().Filter(new Models.Billing.BillingDealsFilter { CreditCardTokenID = token.CreditCardTokenID }).ToListAsync())
                    {
                        billing.ResetToken();

                        await billingDealService.UpdateEntityWithHistory(billing, Messages.CreditCardTokenRemoved, BillingDealOperationCodesEnum.CreditCardTokenRemoved, dbTransaction);
                    }

                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"{nameof(DeleteToken)}: Error while deleting token and updating related data: {ex.Message}");
                    dbTransaction.Rollback();
                }
            }

            return Ok(new OperationResponse(Messages.TokenDeleted, StatusEnum.Success, guid));
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        protected internal async Task<ActionResult<OperationResponse>> CreateTokenInternal(Terminal terminal, TokenRequest model, DocumentOriginEnum origin = DocumentOriginEnum.UI, bool doNotCreateInitialDealAndDbRecord = false)
        {
            //// TODO: caching
            //var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));

            //// TODO: caching
            //var systemSettings = await systemSettingsService.GetSystemSettings();

            //// merge system settings with terminal settings
            //mapper.Map(systemSettings, terminal);
            Consumer consumer = null;

            TokenTerminalSettingsValidator.Validate(terminal, model);

            if (model.ConsumerID != null)
            {
                consumer = EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == model.ConsumerID), "Consumer");

                // TODO: enable if needed
                //if (!string.IsNullOrWhiteSpace(consumer.ConsumerNationalID) && !string.IsNullOrWhiteSpace(model.CardOwnerNationalID) && !consumer.ConsumerNationalID.Equals(model.CardOwnerNationalID, StringComparison.InvariantCultureIgnoreCase))
                //{
                //    throw new EntityConflictException(Messages.ConsumerNatIdIsNotEqTranNatId, "Consumer");
                //}
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(model.CardOwnerNationalID))
                {
                    consumer = await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerNationalID == model.CardOwnerNationalID);
                }
                else if (!string.IsNullOrWhiteSpace(model.ConsumerEmail))
                {
                    consumer = await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerEmail == model.ConsumerEmail);
                }

                if (consumer != null)
                {
                    model.ConsumerID = consumer.ConsumerID;
                }
            }

            var storageData = mapper.Map<CreditCardTokenKeyVault>(model);
            var dbData = mapper.Map<CreditCardTokenDetails>(model);

            storageData.TerminalID = dbData.TerminalID = terminal.TerminalID;
            storageData.MerchantID = dbData.MerchantID = terminal.MerchantID;

            if (!terminal.Settings.DoNotCreateSaveTokenInitialDeal && !doNotCreateInitialDealAndDbRecord)
            {
                if (!(terminal.Settings.J5Allowed == true))
                {
                    List<SharedHelpers.Error> errors = new List<SharedHelpers.Error>();
                    errors.Add(new SharedHelpers.Error($"{nameof(terminal.Settings.J5Allowed)}", Messages.J5NotAllowed));
                    throw new BusinessException(errors.First().Description, errors);
                }

                // initial transaction
                var transaction = new PaymentTransaction();
                mapper.Map(terminal, transaction);

                transaction.ApplyAuditInfo(httpContextAccessor);

                transaction.CardPresence = CardPresenceEnum.CardNotPresent; // TODO
                transaction.JDealType = JDealTypeEnum.J5;
                transaction.DocumentOrigin = origin;
                transaction.SpecialTransactionType = SpecialTransactionTypeEnum.InitialDeal;
                storageData.InitialTransactionID = transaction.PaymentTransactionID;
                dbData.InitialTransactionID = transaction.PaymentTransactionID;

                mapper.Map(storageData, transaction.CreditCardDetails);

                // terminal settings

                var terminalProcessor = ValidateExists(
                    terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Processor),
                    Messages.ProcessorNotDefined);

                var processor = processorResolver.GetProcessor(terminalProcessor);

                var processorSettings = processorResolver.GetProcessorTerminalSettings(terminalProcessor, terminalProcessor.Settings);
                mapper.Map(processorSettings, transaction);

                transaction.Calculate();

                using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
                {
                    var check = await CheckDuplicateTransaction(terminal, model.CardNumber, dbTransaction);
                    if (check != null)
                    {
                        await dbTransaction.RollbackAsync();
                        return check;
                    }

                    await transactionsService.CreateEntity(transaction, dbTransaction);
                    await dbTransaction.CommitAsync();
                }

                // create transaction in processor (Shva)
                try
                {
                    var processorRequest = mapper.Map<ProcessorCreateTransactionRequest>(transaction);

                    mapper.Map(model, processorRequest.CreditCardToken);

                    processorRequest.ProcessorSettings = processorSettings;
                    processorRequest.SapakMutavNo = terminal.Settings.RavMutavNumber;
                    var processorResponse = await processor.CreateTransaction(processorRequest);
                    mapper.Map(processorResponse, transaction);
                    mapper.Map(processorResponse, dbData);

                    if (!processorResponse.Success)
                    {
                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedByProcessor, TransactionFinalizationStatusEnum.Initial, rejectionMessage: processorResponse.ErrorMessage, rejectionReason: processorResponse.RejectReasonCode);

                        if (processorResponse.RejectReasonCode == RejectionReasonEnum.AuthorizationCodeRequired)
                        {
                            var message = Messages.AuthorizationCodeRequired.Replace("@number", processorResponse.TelToGetAuthNum).Replace("@retailer", processorResponse.CompRetailerNum);
                            return BadRequest(new OperationResponse(message, StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier) { AdditionalData = JObject.FromObject(new { authorizationCodeRequired = true, message }) });
                        }
                        else
                        {
                            var message = Messages.RejectedByProcessor;
                            if (processorResponse.Errors?.Count() > 0)
                            {
                                message = string.Join(". ", processorResponse.Errors.Select(s => s.Description));
                            }

                            //TODO: show errors in message
                            return BadRequest(new OperationResponse(message, StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, processorResponse.Errors));
                        }
                    }
                    else
                    {
                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.Completed);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Processor Create Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToConfirmByProcesor, TransactionFinalizationStatusEnum.Initial, rejectionReason: RejectionReasonEnum.Unknown, rejectionMessage: ex.Message);

                    return BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionStatusEnum.FailedToConfirmByProcesor.ToString(), (ex as IntegrationException)?.Message));
                }
            }

            // save token itself

            await keyValueStorage.Save(dbData.CreditCardTokenID.ToString(), JsonConvert.SerializeObject(storageData));

            if (!doNotCreateInitialDealAndDbRecord)
            {
                await creditCardTokenService.CreateEntity(dbData);
            }

            if (consumer != null)
            {
                consumer.HasCreditCard = await creditCardTokenService.ConsumerCCTokenExistsAsync(consumer.ConsumerID);
                await consumersService.UpdateEntity(consumer);
            }

            return new OperationResponse(Messages.TokenCreated, StatusEnum.Success, dbData.CreditCardTokenID) { InnerResponse = new OperationResponse(Messages.TokenCreated, StatusEnum.Success, dbData.CreditCardTokenID) };
        }

        private async Task<Terminal> GetTerminal(Guid terminalID)
        {
            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            return terminal;
        }

        private async Task<OperationResponse> CheckDuplicateTransaction(Terminal terminalDetails, string cardNumber, IDbContextTransaction dbContextTransaction)
        {
            bool hasFeature = terminalDetails.EnabledFeatures.Contains(Merchants.Shared.Enums.FeatureEnum.PreventDoubleTansactions);

            if (!hasFeature)
            {
                return null;
            }

            DateTime? threshold = hasFeature ? DateTime.UtcNow.AddMinutes(-(terminalDetails.Settings.MinutesToWaitBetDuplicateTransactions ?? 1)) : (DateTime?)null;

            var res = await transactionsService.CheckDuplicateTransaction(terminalDetails.TerminalID, null, null, threshold, 0m, CreditCardHelpers.GetCardDigits(cardNumber), dbContextTransaction, JDealTypeEnum.J5);

            if (res)
            {
                return new OperationResponse(Messages.DuplicateTransactionIsDetected, (Guid?)null, GetCorrelationID(), "DoubleTansactions", Messages.DuplicateTransactionIsDetected);
            }
            else
            {
                return null;
            }
        }
    }
}