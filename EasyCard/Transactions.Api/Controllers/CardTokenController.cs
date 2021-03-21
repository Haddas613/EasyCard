using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Security.KeyVault.Secrets;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
            ISystemSettingsService systemSettingsService)
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
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = (httpContextAccessor.GetUser().IsAdmin() ? typeof(CreditCardTokenSummaryAdmin) : typeof(CreditCardTokenSummary))
                    .GetObjectMeta(CreditCardSummaryResource.ResourceManager, System.Globalization.CultureInfo.InvariantCulture)
            };
        }

        [HttpPost]
        [ValidateModelState]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        public async Task<ActionResult<OperationResponse>> CreateToken([FromBody] TokenRequest model)
        {
            var tokenResponse = await CreateTokenInternal(model);

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
                    var response = new SummariesResponse<CreditCardTokenSummary>();
                    response.Data = await mapper.ProjectTo<CreditCardTokenSummary>(query.ApplyPagination(filter)).Future().ToListAsync();
                    response.NumberOfRecords = numberOfRecordsFuture.Value;

                    return Ok(response);
                }
            }
        }

        [HttpDelete]
        [Route("{key}")]
        public async Task<ActionResult<OperationResponse>> DeleteToken(string key)
        {
            var guid = new Guid(key);
            var token = EnsureExists(await creditCardTokenService.GetTokens().FirstOrDefaultAsync(t => t.CreditCardTokenID == guid));

            var terminal = EnsureExists(await terminalsService.GetTerminals().Where(d => d.TerminalID == token.TerminalID).FirstOrDefaultAsync());

            try
            {
                await keyValueStorage.Delete(key);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"{nameof(DeleteToken)}: Error while deleting token from keyvalue storage. Message: {e.Message}");
            }

            token.Active = false;
            await creditCardTokenService.UpdateEntity(token);

            return Ok(new OperationResponse(Messages.TokenDeleted, StatusEnum.Success, guid));
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        protected internal async Task<ActionResult<OperationResponse>> CreateTokenInternal(TokenRequest model)
        {
            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            TokenTerminalSettingsValidator.Validate(terminal.Settings, model);

            if (model.ConsumerID != null)
            {
                var consumer = EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == model.ConsumerID && d.TerminalID == terminal.TerminalID), "Consumer");

                if (!string.IsNullOrWhiteSpace(consumer.ConsumerNationalID) && !string.IsNullOrWhiteSpace(model.CardOwnerNationalID) && !consumer.ConsumerNationalID.Equals(model.CardOwnerNationalID, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new EntityConflictException(Messages.ConsumerNatIdIsNotEqTranNatId, "Consumer");
                }
            }
            else
            {
                var consumer = await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerNationalID == model.CardOwnerNationalID && d.TerminalID == terminal.TerminalID);
                if (consumer != null)
                {
                    model.ConsumerID = consumer.ConsumerID;
                }
            }

            var storageData = mapper.Map<CreditCardTokenKeyVault>(model);
            var dbData = mapper.Map<CreditCardTokenDetails>(model);

            storageData.TerminalID = dbData.TerminalID = terminal.TerminalID;
            storageData.MerchantID = dbData.MerchantID = terminal.MerchantID;

            // initial transaction
            var transaction = new PaymentTransaction();
            mapper.Map(terminal, transaction);

            transaction.MerchantIP = GetIP();
            transaction.CorrelationId = GetCorrelationID();

            transaction.CardPresence = CardPresenceEnum.CardNotPresent; // TODO
            transaction.JDealType = JDealTypeEnum.J5;

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

            await transactionsService.CreateEntity(transaction);

            // create transaction in processor (Shva)
            try
            {
                var processorRequest = mapper.Map<ProcessorCreateTransactionRequest>(transaction);

                mapper.Map(model, processorRequest.CreditCardToken);

                processorRequest.ProcessorSettings = processorSettings;

                var processorResponse = await processor.CreateTransaction(processorRequest);
                mapper.Map(processorResponse, transaction);
                mapper.Map(processorResponse, dbData);

                if (!processorResponse.Success)
                {
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedByProcessor, TransactionFinalizationStatusEnum.Initial, rejectionMessage: processorResponse.ErrorMessage, rejectionReason: processorResponse.RejectReasonCode);

                    return BadRequest(new OperationResponse($"{Messages.RejectedByProcessor}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, processorResponse.Errors));
                }
                else
                {
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.ConfirmedByProcessor);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Processor Create Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToConfirmByProcesor, TransactionFinalizationStatusEnum.Initial, rejectionReason: RejectionReasonEnum.Unknown, rejectionMessage: ex.Message);

                return BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionStatusEnum.FailedToConfirmByProcesor.ToString(), (ex as IntegrationException)?.Message));
            }

            // save token itself

            await keyValueStorage.Save(dbData.CreditCardTokenID.ToString(), JsonConvert.SerializeObject(storageData));

            await creditCardTokenService.CreateEntity(dbData);

            return new OperationResponse(Messages.TokenCreated, StatusEnum.Success, dbData.CreditCardTokenID);
        }
    }
}