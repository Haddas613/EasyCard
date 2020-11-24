using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
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
using Shared.Business.Extensions;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.KeyValueStorage;
using Shared.Helpers.Security;
using Shared.Integration.Exceptions;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using Swashbuckle.AspNetCore.Filters;
using Transactions.Api.Extensions.Filtering;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Services;
using Transactions.Api.Swagger;
using Transactions.Api.Validation;
using Transactions.Api.Validation.Options;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Transactions.Shared;
using Transactions.Shared.Enums;
using Transactions.Api.Extensions;
using SharedBusiness = Shared.Business;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/transactions")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
    [ApiController]
    public class TransactionsApiController : ApiControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly IMapper mapper;
        private readonly IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage;
        private readonly IAggregatorResolver aggregatorResolver;
        private readonly IProcessorResolver processorResolver;
        private readonly ILogger logger;
        private readonly ApplicationSettings appSettings;
        private readonly ICreditCardTokenService creditCardTokenService;
        private readonly IBillingDealService billingDealService;
        private readonly ITerminalsService terminalsService;
        private readonly IConsumersService consumersService;
        private readonly CardTokenController cardTokenController;
        private readonly IPaymentRequestsService paymentRequestsService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly IInvoiceService invoiceService;
        private readonly ISystemSettingsService systemSettingsService;

        public TransactionsApiController(
            ITransactionsService transactionsService,
            IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage,
            IMapper mapper,
            IAggregatorResolver aggregatorResolver,
            IProcessorResolver processorResolver,
            ITerminalsService terminalsService,
            ILogger<TransactionsApiController> logger,
            IOptions<ApplicationSettings> appSettings,
            ICreditCardTokenService creditCardTokenService,
            IBillingDealService billingDealService,
            IConsumersService consumersService,
            CardTokenController cardTokenController,
            IInvoiceService invoiceService,
            IPaymentRequestsService paymentRequestsService,
            IHttpContextAccessorWrapper httpContextAccessor,
            ISystemSettingsService systemSettingsService)
        {
            this.transactionsService = transactionsService;
            this.keyValueStorage = keyValueStorage;
            this.mapper = mapper;

            this.aggregatorResolver = aggregatorResolver;
            this.processorResolver = processorResolver;
            this.terminalsService = terminalsService;
            this.logger = logger;
            this.appSettings = appSettings.Value;
            this.creditCardTokenService = creditCardTokenService;
            this.billingDealService = billingDealService;
            this.consumersService = consumersService;
            this.cardTokenController = cardTokenController;
            this.invoiceService = invoiceService;
            this.paymentRequestsService = paymentRequestsService;
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
                Columns = typeof(TransactionSummary)
                    .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Select(d => d.GetColMeta(TransactionSummaryResource.ResourceManager, System.Globalization.CultureInfo.InvariantCulture))
                    .ToDictionary(d => d.Key)
            };
        }

        [HttpGet]
        [Route("$grouped")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<IEnumerable<GroupedSummariesResponse<TransactionSummary>>>> GetTransactionsGrouped([FromQuery]Guid? terminalID)
        {
            Debug.WriteLine(User);
            var merchantID = User.GetMerchantID();
            var userIsTerminal = User.IsTerminal();

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var query = await transactionsService.GetGroupedTransactionSummaries(terminalID, dbTransaction);

                var results = from p in query
                              group p by new { p.TransactionDate, p.NumberOfRecords } into g
                              select new GroupedSummariesResponse<TransactionSummary>
                              {
                                  GroupValue = g.Key,
                                  Data = mapper.Map<IEnumerable<TransactionSummary>>(g.ToList())
                              };

                return Ok(results);
            }
        }

        [HttpGet]
        [Route("{transactionID}")]
        public async Task<ActionResult<TransactionResponse>> GetTransaction([FromRoute] Guid transactionID)
        {
            Debug.WriteLine(User);

            var transaction = mapper.Map<TransactionResponse>(EnsureExists(await transactionsService.GetTransactions()
                .FirstOrDefaultAsync(m => m.PaymentTransactionID == transactionID)));

            return Ok(transaction);
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("{transactionID}/history")]
        [Authorize(Policy = Policy.AnyAdmin)]
        public async Task<ActionResult<TransactionResponse>> GetTransactionHistory([FromRoute] Guid transactionID)
        {
            var transaction = EnsureExists(await transactionsService.GetTransactions()
                .Where(m => m.PaymentTransactionID == transactionID).Select(d => d.PaymentTransactionID).FirstOrDefaultAsync());

            var query = transactionsService.GetTransactionHistory(transactionID);

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<Models.Transactions.TransactionHistory> { NumberOfRecords = await query.CountAsync() };

                response.Data = await mapper.ProjectTo<Models.Transactions.TransactionHistory>(query).ToListAsync();

                return Ok(response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<TransactionSummary>>> GetTransactions([FromQuery] TransactionsFilter filter)
        {
            Debug.WriteLine(User);
            var merchantID = User.GetMerchantID();
            var userIsTerminal = User.IsTerminal();

            TransactionsFilterValidator.ValidateFilter(filter, new TransactionFilterValidationOptions { MaximumPageSize = appSettings.FiltersGlobalPageSizeLimit });

            var query = transactionsService.GetTransactions().AsNoTracking().Filter(filter);

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<TransactionSummary> { NumberOfRecords = await query.CountAsync() };

                query = query.OrderByDynamic(filter.SortBy ?? nameof(PaymentTransaction.PaymentTransactionID), filter.OrderByDirection).ApplyPagination(filter, appSettings.FiltersGlobalPageSizeLimit);

                // TODO: validate generated sql
                var sql = query.ToSql();

                // TODO: try to remove ProjectTo
                response.Data = await mapper.ProjectTo<TransactionSummary>(query).ToListAsync();

                return Ok(response);
            }
        }

        /// <summary>
        /// Create the charge based on credit card or previously stored credit card token (J4 deal)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("create")]
        [ValidateModelState]
        [SwaggerRequestExample(typeof(CreateTransactionRequest), typeof(CreateTransactionRequestExample))]
        public async Task<ActionResult<OperationResponse>> CreateTransaction([FromBody] CreateTransactionRequest model)
        {
            Debug.WriteLine(User);
            var merchantID = User.GetMerchantID();
            var userIsTerminal = User.IsTerminal();

            if (model.SaveCreditCard == true)
            {
                if (model.CreditCardToken != null)
                {
                    throw new BusinessException(Messages.WhenSpecifiedTokenCCDIsNotValid);
                }

                var tokenRequest = mapper.Map<TokenRequest>(model.CreditCardSecureDetails);
                mapper.Map(model, tokenRequest);

                var tokenResponse = await cardTokenController.CreateTokenInternal(tokenRequest);

                var tokenResponseOperation = tokenResponse.GetOperationResponse();

                if (!(tokenResponseOperation?.Status == StatusEnum.Success))
                {
                    return tokenResponse;
                }

                model.CreditCardToken = tokenResponseOperation.EntityUID;
                model.CreditCardSecureDetails = null;
            }

            if (model.CreditCardToken != null)
            {
                if (model.CreditCardSecureDetails != null)
                {
                    throw new BusinessException(Messages.WhenSpecifiedTokenCCDetailsShouldBeOmitted);
                }

                var token = EnsureExists(await keyValueStorage.Get(model.CreditCardToken.ToString()), "CreditCardToken");
                return await ProcessTransaction(model, token, specialTransactionType: SpecialTransactionTypeEnum.RegularDeal);
            }
            else
            {
                return await ProcessTransaction(model, null);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("prcreate")]
        [ValidateModelState]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Policy = Policy.AnyAdmin)]
        public async Task<ActionResult<OperationResponse>> PRCreateTransaction([FromBody] PRCreateTransactionRequest prmodel)
        {
            var dbPaymentRequest = EnsureExists(await paymentRequestsService.GetPaymentRequests().FirstOrDefaultAsync(m => m.PaymentRequestID == prmodel.PaymentRequestID));

            CreateTransactionRequest model = new CreateTransactionRequest();

            mapper.Map(dbPaymentRequest, model);
            mapper.Map(prmodel, model);

            var createResult = await ProcessTransaction(model, null);

            if (createResult.Value.Status == StatusEnum.Success)
            {
                await paymentRequestsService.UpdateEntityWithStatus(dbPaymentRequest, PaymentRequestStatusEnum.Payed, paymentTransactionID: createResult.Value.EntityUID.Value);
            }
            else
            {
                await paymentRequestsService.UpdateEntityWithStatus(dbPaymentRequest, PaymentRequestStatusEnum.PaymentFailed, paymentTransactionID: createResult.Value.EntityUID.Value);
            }

            return createResult;
        }

        /// <summary>
        /// Blocking funds on credit card (J5 deal)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("blocking")]
        [ValidateModelState]
        public async Task<ActionResult<OperationResponse>> BlockCreditCard([FromBody] BlockCreditCardRequest model)
        {
            var transaction = mapper.Map<CreateTransactionRequest>(model);

            // Does it have sense to use J5 together with Token?
            if (!string.IsNullOrWhiteSpace(model.CreditCardToken))
            {
                var token = EnsureExists(await keyValueStorage.Get(model.CreditCardToken), "CreditCardToken");
                return await ProcessTransaction(transaction, token, JDealTypeEnum.J5);
            }
            else
            {
                return await ProcessTransaction(transaction, null, JDealTypeEnum.J5);
            }
        }

        /// <summary>
        /// Check if credit card is valid
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("checking")]
        [ValidateModelState]
        public async Task<ActionResult<OperationResponse>> CheckCreditCard([FromBody] CheckCreditCardRequest model)
        {
            var transaction = mapper.Map<CreateTransactionRequest>(model);
            transaction.TransactionAmount = 1;

            return await ProcessTransaction(transaction, null, JDealTypeEnum.J2);
        }

        /// <summary>
        /// Refund request
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("refund")]
        [ValidateModelState]
        public async Task<ActionResult<OperationResponse>> Refund([FromBody] RefundRequest model)
        {
            var transaction = mapper.Map<CreateTransactionRequest>(model);

            if (!string.IsNullOrWhiteSpace(model.CreditCardToken))
            {
                var token = EnsureExists(await keyValueStorage.Get(model.CreditCardToken), "CreditCardToken");
                return await ProcessTransaction(transaction, token, JDealTypeEnum.J4, SpecialTransactionTypeEnum.Refund);
            }
            else
            {
                return await ProcessTransaction(transaction, null, JDealTypeEnum.J4, SpecialTransactionTypeEnum.Refund);
            }
        }

        ///// <summary>
        ///// Initial request for set of billing trnsactions
        ///// </summary>
        //[HttpPost]
        //[ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        //[Route("initalBillingDeal")]
        //[ValidateModelState]
        //public async Task<ActionResult<OperationResponse>> InitalBillingDeal([FromBody] InitalBillingDealRequest model)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Billing deal
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("nextBillingDeal")]
        [ValidateModelState]
        public async Task<ActionResult<OperationResponse>> NextBillingDeal([FromBody] NextBillingDealRequest model)
        {
            var billingDeal = EnsureExists(await billingDealService.GetBillingDeals().FirstOrDefaultAsync(d => d.BillingDealID == model.BillingDealID));

            var token = EnsureExists(await keyValueStorage.Get(billingDeal.CreditCardToken.ToString()), "CreditCardToken");

            var transaction = mapper.Map<CreateTransactionRequest>(model);

            transaction.CardPresence = CardPresenceEnum.CardNotPresent;
            transaction.Currency = billingDeal.Currency;
            transaction.TransactionType = TransactionTypeEnum.RegularDeal;
            transaction.CreditCardToken = billingDeal.CreditCardToken;

            return await ProcessTransaction(transaction, token, specialTransactionType: SpecialTransactionTypeEnum.RegularDeal, initialTransactionID: billingDeal.InitialTransactionID, billingDealID: billingDeal.BillingDealID);
        }

        private async Task<ActionResult<OperationResponse>> ProcessTransaction(CreateTransactionRequest model, CreditCardTokenKeyVault token, JDealTypeEnum jDealType = JDealTypeEnum.J4, SpecialTransactionTypeEnum specialTransactionType = SpecialTransactionTypeEnum.RegularDeal, Guid? initialTransactionID = null, Guid? billingDealID = null)
        {
            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            TransactionTerminalSettingsValidator.Validate(terminal.Settings, model, token, jDealType, specialTransactionType, initialTransactionID);

            var transaction = mapper.Map<PaymentTransaction>(model);

            // NOTE: this is security assignment
            mapper.Map(terminal, transaction);

            transaction.SpecialTransactionType = specialTransactionType;
            transaction.JDealType = jDealType;
            transaction.BillingDealID = billingDealID;
            transaction.InitialTransactionID = initialTransactionID;

            if (transaction.DealDetails == null)
            {
                transaction.DealDetails = new Business.Entities.DealDetails();
            }

            if (model.IssueInvoice == true && model.InvoiceDetails == null)
            {
                model.InvoiceDetails = new SharedIntegration.Models.Invoicing.InvoiceDetails { InvoiceType = terminal.InvoiceSettings.DefaultInvoiceType.GetValueOrDefault() };
            }

            // Update card information based on token
            CreditCardTokenDetails dbToken = null;
            if (token != null)
            {
                if (token.TerminalID != terminal.TerminalID)
                {
                    throw new EntityNotFoundException(SharedBusiness.Messages.ApiMessages.EntityNotFound, "CreditCardToken", null);
                }

                mapper.Map(token, transaction.CreditCardDetails);

                dbToken = EnsureExists(await creditCardTokenService.GetTokens().FirstOrDefaultAsync(d => d.CreditCardTokenID == model.CreditCardToken));

                if (transaction.InitialTransactionID == null)
                {
                    transaction.InitialTransactionID = dbToken.InitialTransactionID;
                }

                if (transaction.DealDetails?.ConsumerID == null)
                {
                    transaction.DealDetails.ConsumerID = dbToken.ConsumerID;
                }
            }
            else
            {
                mapper.Map(model.CreditCardSecureDetails, transaction.CreditCardDetails);
            }

            // Check consumer
            var consumer = transaction.DealDetails.ConsumerID != null ? EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == transaction.DealDetails.ConsumerID && d.TerminalID == terminal.TerminalID), "Consumer") : null;

            if (consumer != null)
            {
                if (dbToken != null)
                {
                    if (consumer.ConsumerID != dbToken.ConsumerID)
                    {
                        throw new EntityNotFoundException(SharedBusiness.Messages.ApiMessages.EntityNotFound, "CreditCardToken", null);
                    }

                    if (!string.IsNullOrWhiteSpace(consumer.ConsumerNationalID) && !string.IsNullOrWhiteSpace(dbToken.CardOwnerNationalID) && !consumer.ConsumerNationalID.Equals(dbToken.CardOwnerNationalID, StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new EntityConflictException(Messages.ConsumerNatIdIsNotEqTranNatId, "Consumer");
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(consumer.ConsumerNationalID) && !string.IsNullOrWhiteSpace(model.CreditCardSecureDetails.CardOwnerNationalID) && !consumer.ConsumerNationalID.Equals(model.CreditCardSecureDetails.CardOwnerNationalID, StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new EntityConflictException(Messages.ConsumerNatIdIsNotEqTranNatId, "Consumer");
                    }
                }
            }

            // Update details if needed
            transaction.DealDetails.UpdateDealDetails(consumer, terminal.Settings, transaction);
            if (transaction.IssueInvoice)
            {
                model.InvoiceDetails.UpdateInvoiceDetails(terminal.InvoiceSettings);
            }

            transaction.Calculate();

            transaction.MerchantIP = GetIP();
            transaction.CorrelationId = GetCorrelationID();

            var terminalAggregator = ValidateExists(
                terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator),
                Messages.AggregatorNotDefined);

            var terminalProcessor = ValidateExists(
                terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Processor),
                Messages.ProcessorNotDefined);

            transaction.AggregatorID = terminalAggregator.ExternalSystemID;
            transaction.ProcessorID = terminalProcessor.ExternalSystemID;

            var aggregator = aggregatorResolver.GetAggregator(terminalAggregator);
            var processor = processorResolver.GetProcessor(terminalProcessor);

            var aggregatorSettings = aggregatorResolver.GetAggregatorTerminalSettings(terminalAggregator, terminalAggregator.Settings);
            mapper.Map(aggregatorSettings, transaction);

            var processorSettings = processorResolver.GetProcessorTerminalSettings(terminalProcessor, terminalProcessor.Settings);
            mapper.Map(processorSettings, transaction);

            await transactionsService.CreateEntity(transaction);

            // create transaction in aggregator (Clearing House)
            if (aggregator.ShouldBeProcessedByAggregator(transaction.TransactionType, transaction.SpecialTransactionType, transaction.JDealType))
            {
                try
                {
                    var aggregatorRequest = mapper.Map<AggregatorCreateTransactionRequest>(transaction);
                    aggregatorRequest.AggregatorSettings = aggregatorSettings;

                    var aggregatorResponse = await aggregator.CreateTransaction(aggregatorRequest);
                    mapper.Map(aggregatorResponse, transaction);

                    if (!aggregatorResponse.Success)
                    {
                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedByAggregator, rejectionMessage: aggregatorResponse.ErrorMessage, rejectionReason: aggregatorResponse.RejectReasonCode);

                        return BadRequest(new OperationResponse($"{Messages.RejectedByAggregator}: {aggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, aggregatorResponse.Errors));
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

                    return BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionStatusEnum.FailedToConfirmByAggregator.ToString(), (ex as IntegrationException)?.Message));
                }
            }

            BadRequestObjectResult processorFailedRsponse = null;

            // create transaction in processor (Shva)
            try
            {
                var processorRequest = mapper.Map<ProcessorCreateTransactionRequest>(transaction);

                if (token != null)
                {
                    mapper.Map(token, processorRequest.CreditCardToken);
                    mapper.Map(dbToken.ShvaInitialTransactionDetails, processorRequest.InitialDeal, typeof(ShvaInitialTransactionDetails), typeof(Shva.Models.InitDealResultModel)); // TODO: remove direct Shva reference
                }
                else
                {
                    mapper.Map(model.CreditCardSecureDetails, processorRequest.CreditCardToken);
                }

                processorRequest.ProcessorSettings = processorSettings;

                var processorResponse = await processor.CreateTransaction(processorRequest);
                mapper.Map(processorResponse, transaction);

                if (!processorResponse.Success)
                {
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedByProcessor, TransactionFinalizationStatusEnum.Initial, rejectionMessage: processorResponse.ErrorMessage, rejectionReason: processorResponse.RejectReasonCode);

                    processorFailedRsponse = BadRequest(new OperationResponse($"{Messages.RejectedByProcessor}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, processorResponse.Errors));
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

                processorFailedRsponse = BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionStatusEnum.FailedToConfirmByProcesor.ToString(), (ex as IntegrationException)?.Message));
            }

            if (aggregator.ShouldBeProcessedByAggregator(transaction.TransactionType, transaction.SpecialTransactionType, transaction.JDealType))
            {
                // reject to clearing house in case of shva error
                if (processorFailedRsponse != null)
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

                            return BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}: {aggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier));
                        }
                        else
                        {
                            await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.CanceledByAggregator);

                            return processorFailedRsponse;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Aggregator Cancel Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                        await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.FailedToCancelByAggregator);

                        return BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionFinalizationStatusEnum.FailedToCancelByAggregator.ToString(), (ex as IntegrationException)?.Message));
                    }
                }

                // commit transaction in aggregator (Clearing House)
                else
                {
                    try
                    {
                        var aggregatorRequest = mapper.Map<AggregatorCreateTransactionRequest>(transaction);

                        var commitAggregatorRequest = mapper.Map<AggregatorCommitTransactionRequest>(transaction);

                        commitAggregatorRequest.AggregatorSettings = terminalAggregator.Settings;

                        var commitAggregatorResponse = await aggregator.CommitTransaction(commitAggregatorRequest);
                        mapper.Map(commitAggregatorResponse, transaction);

                        if (!commitAggregatorResponse.Success)
                        {
                            // NOTE: In case of failed commit, transaction should not be transmitted to Shva

                            logger.LogError($"Aggregator Commit Transaction request error. TransactionID: {transaction.PaymentTransactionID}");

                            await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToCommitByAggregator, rejectionMessage: commitAggregatorResponse.ErrorMessage, rejectionReason: commitAggregatorResponse.RejectReasonCode);

                            return BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}: {commitAggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, commitAggregatorResponse.Errors));
                        }
                        else
                        {
                            await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.CommitedByAggregator);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Aggregator Commit Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToCommitByAggregator, rejectionReason: RejectionReasonEnum.Unknown, rejectionMessage: ex.Message);

                        return BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionStatusEnum.FailedToCommitByAggregator.ToString(), (ex as IntegrationException)?.Message));
                    }
                }
            }
            else if (processorFailedRsponse != null)
            {
                return processorFailedRsponse;
            }

            var endResponse = new OperationResponse(Messages.TransactionCreated, StatusEnum.Success, transaction.PaymentTransactionID);

            // TODO: validate InvoiceDetails
            if (model.IssueInvoice == true)
            {
                using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
                {
                    try
                    {
                        Invoice invoiceRequest = new Invoice();
                        mapper.Map(transaction, invoiceRequest);
                        invoiceRequest.InvoiceDetails = model.InvoiceDetails;

                        invoiceRequest.MerchantID = terminal.MerchantID;

                        invoiceRequest.ApplyAuditInfo(httpContextAccessor);

                        await invoiceService.CreateEntity(invoiceRequest, dbTransaction: dbTransaction);

                        endResponse.InnerResponse = new OperationResponse(Transactions.Shared.Messages.InvoiceCreated, StatusEnum.Success, invoiceRequest.InvoiceID);

                        transaction.InvoiceID = invoiceRequest.InvoiceID;

                        await transactionsService.UpdateEntity(transaction, Messages.InvoiceCreated, TransactionOperationCodesEnum.InvoiceCreated, dbTransaction: dbTransaction);

                        await dbTransaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Failed to create invoice. TransactionID: {transaction.PaymentTransactionID}");

                        endResponse.InnerResponse = new OperationResponse($"{Messages.FailedToCreateInvoice}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, "FailedToCreateInvoice", ex.Message);

                        await dbTransaction.RollbackAsync();
                    }
                }
            }

            return CreatedAtAction(nameof(GetTransaction), new { transactionID = transaction.PaymentTransactionID }, endResponse);
        }
    }
}