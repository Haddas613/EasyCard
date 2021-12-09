using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Api;
using Shared.Api.Configuration;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Api.Validation;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.Email;
using Shared.Helpers.KeyValueStorage;
using Shared.Helpers.Queue;
using Shared.Helpers.Security;
using Shared.Helpers.Templating;
using Shared.Integration.Exceptions;
using Shared.Integration.Models;
using Swashbuckle.AspNetCore.Filters;
using Transactions.Api.Extensions;
using Transactions.Api.Extensions.Filtering;
using Transactions.Api.Models.Billing;
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
using Z.EntityFramework.Plus;
using Merchants.Business.Entities.Terminal;
using Shared.Integration.ExternalSystems;
using Shared.Integration;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.SignalR;
using Shared.Helpers.Services;
using SharedBusiness = Shared.Business;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Controllers
{
    /// <summary>
    /// Payment transactions API
    /// </summary>
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/transactions")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
    [ApiController]
    public partial class TransactionsApiController : ApiControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly IMapper mapper;
        private readonly IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage;
        private readonly IAggregatorResolver aggregatorResolver;
        private readonly IProcessorResolver processorResolver;
        private readonly ILogger logger;
        private readonly ApplicationSettings appSettings;
        private readonly ApiSettings apiSettings;
        private readonly ICreditCardTokenService creditCardTokenService;
        private readonly IBillingDealService billingDealService;
        private readonly ITerminalsService terminalsService;
        private readonly IConsumersService consumersService;
        private readonly CardTokenController cardTokenController;
        private readonly BillingController billingController;
        private readonly IPaymentRequestsService paymentRequestsService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly IInvoiceService invoiceService;
        private readonly ISystemSettingsService systemSettingsService;
        private readonly IMerchantsService merchantsService;
        private readonly IQueue invoiceQueue;
        private readonly IEmailSender emailSender;
        private readonly IMetricsService metrics;
        private readonly IPaymentIntentService paymentIntentService;
        private readonly InvoicingController invoicingController;

        private readonly IHubContext<Hubs.TransactionsHub, Shared.Hubs.ITransactionsHub> transactionsHubContext;

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
            ISystemSettingsService systemSettingsService,
            IMerchantsService merchantsService,
            IQueueResolver queueResolver,
            IEmailSender emailSender,
            IOptions<ApiSettings> apiSettings,
            IMetricsService metrics,
            IPaymentIntentService paymentIntentService,
            IHubContext<Hubs.TransactionsHub, Shared.Hubs.ITransactionsHub> transactionsHubContext,
            BillingController billingController,
            InvoicingController invoicingController)
        {
            this.transactionsService = transactionsService;
            this.keyValueStorage = keyValueStorage;
            this.mapper = mapper;
            this.apiSettings = apiSettings.Value;
            this.aggregatorResolver = aggregatorResolver;
            this.processorResolver = processorResolver;
            this.terminalsService = terminalsService;
            this.logger = logger;
            this.appSettings = appSettings.Value;
            this.creditCardTokenService = creditCardTokenService;
            this.billingDealService = billingDealService;
            this.consumersService = consumersService;
            this.cardTokenController = cardTokenController;
            this.billingController = billingController;
            this.invoiceService = invoiceService;
            this.paymentRequestsService = paymentRequestsService;
            this.httpContextAccessor = httpContextAccessor;
            this.systemSettingsService = systemSettingsService;
            this.merchantsService = merchantsService;
            this.invoiceQueue = queueResolver.GetQueue(QueueResolver.InvoiceQueue);
            this.emailSender = emailSender;
            this.metrics = metrics;
            this.paymentIntentService = paymentIntentService;
            this.transactionsHubContext = transactionsHubContext;
            this.invoicingController = invoicingController;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = (httpContextAccessor.GetUser().IsAdmin() ? typeof(TransactionSummaryAdmin) : typeof(TransactionSummary))
                    .GetObjectMeta(TransactionSummaryResource.ResourceManager, CurrentCulture)
            };
        }

        [HttpGet]
        [Route("$grouped")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<IEnumerable<GroupedSummariesResponse<TransactionSummary>>>> GetTransactionsGrouped([FromQuery] Guid? terminalID)
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

        /// <summary>
        /// Get payment transaction details
        /// </summary>
        /// <param name="transactionID">Payment transaction UUId</param>
        /// <returns>Transaction details</returns>
        [HttpGet]
        [Route("{transactionID}")]
        [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
        [SwaggerResponseExample(201, typeof(GetTransactionResponseExample))]
        public async Task<ActionResult<TransactionResponse>> GetTransaction([FromRoute] Guid transactionID)
        {
            Debug.WriteLine(User);

            if (httpContextAccessor.GetUser().IsAdmin())
            {
                var tr = EnsureExists(
                    await transactionsService.GetTransactions().FirstOrDefaultAsync(m => m.PaymentTransactionID == transactionID));
                var transaction = mapper.Map<TransactionResponseAdmin>(tr);

                // TODO: find another way to map it
                if (tr.AggregatorID == 60)
                {
                    transaction.ClearingHouseTransactionDetails = null;
                }
                else if (tr.AggregatorID == 10)
                {
                    transaction.UpayTransactionDetails = null;
                }
                else
                {
                    transaction.ClearingHouseTransactionDetails = null;
                    transaction.UpayTransactionDetails = null;
                }

                var terminal = EnsureExists(await terminalsService.GetTerminal(transaction.TerminalID.Value));

                if (transaction.JDealType == JDealTypeEnum.J5)
                {
                    transaction.TransactionJ5ExpiredDate = DateTime.Now.AddDays(terminal.Settings.J5ExpirationDays);
                }

                if (transaction.AllowTransmissionCancellation)
                {
                    var terminalAggregator = terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator);

                    if (terminalAggregator != null)
                    {
                        var aggregator = aggregatorResolver.GetAggregator(terminalAggregator);
                        transaction.AllowTransmissionCancellation = aggregator?.AllowTransmissionCancellation() ?? transaction.AllowTransmissionCancellation;
                    }
                }

                transaction.TerminalName = terminal.Label;
                transaction.MerchantName = terminal.Merchant.BusinessName;
                return Ok(transaction);
            }
            else
            {
                var transaction = mapper.Map<TransactionResponse>(EnsureExists(
                    await transactionsService.GetTransactions().FirstOrDefaultAsync(m => m.PaymentTransactionID == transactionID)));

                var terminal = EnsureExists(await terminalsService.GetTerminal(transaction.TerminalID.Value));
                transaction.TerminalName = terminal.Label;

                if (transaction.AllowTransmissionCancellation)
                {
                    var terminalAggregator = terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator);

                    if (terminalAggregator != null)
                    {
                        var aggregator = aggregatorResolver.GetAggregator(terminalAggregator);
                        transaction.AllowTransmissionCancellation = aggregator?.AllowTransmissionCancellation() ?? transaction.AllowTransmissionCancellation;
                    }
                }

                return Ok(transaction);
            }
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("{transactionID}/history")]
        [Authorize(Policy = Policy.AnyAdmin)]
        public async Task<ActionResult<SummariesResponse<Models.Transactions.TransactionHistory>>> GetTransactionHistory([FromRoute] Guid transactionID)
        {
            var transaction = EnsureExists(await transactionsService.GetTransactions()
                .Where(m => m.PaymentTransactionID == transactionID).Select(d => d.PaymentTransactionID).FirstOrDefaultAsync());

            var query = transactionsService.GetTransactionHistory(transactionID);

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var numberOfRecords = query.DeferredCount().FutureValue();
                var response = new SummariesResponse<Models.Transactions.TransactionHistory>();

                response.Data = await mapper.ProjectTo<Models.Transactions.TransactionHistory>(query.OrderByDescending(t => t.OperationDate)).Future().ToListAsync();
                response.NumberOfRecords = numberOfRecords.Value;

                return Ok(response);
            }
        }

        /// <summary>
        /// Get payment transactions list using filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<SummariesAmountResponse<TransactionSummary>>> GetTransactions([FromQuery] TransactionsFilter filter)
        {
            Debug.WriteLine(User);
            var merchantID = User.GetMerchantID();
            var userIsTerminal = User.IsTerminal();

            TransactionsFilterValidator.ValidateFilter(filter, new TransactionFilterValidationOptions { MaximumPageSize = appSettings.FiltersGlobalPageSizeLimit });

            var query = transactionsService.GetTransactions().AsNoTracking().Filter(filter);

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var dataQuery = query.OrderByDynamic(filter.SortBy ?? nameof(PaymentTransaction.PaymentTransactionID), filter.SortDesc).ApplyPagination(filter, appSettings.FiltersGlobalPageSizeLimit);

                var numberOfRecords = query.DeferredCount().FutureValue();
                var totalAmount = query.DeferredSum(e => e.TotalAmount).FutureValue();
                var response = new SummariesAmountResponse<TransactionSummary>();

                if (httpContextAccessor.GetUser().IsAdmin())
                {
                    var summary = await mapper.ProjectTo<TransactionSummaryAdmin>(dataQuery).Future().ToListAsync();

                    var terminalsId = summary.Select(t => t.TerminalID).Distinct();

                    var terminals = await terminalsService.GetTerminals()
                        .Include(t => t.Merchant)
                        .Where(t => terminalsId.Contains(t.TerminalID))
                        .Select(t => new { t.TerminalID, t.Label, t.Merchant.BusinessName })
                        .ToDictionaryAsync(k => k.TerminalID, v => new { v.Label, v.BusinessName });

                    //TODO: Merchant name instead of BusinessName
                    summary.ForEach(s =>
                    {
                        if (terminals.ContainsKey(s.TerminalID))
                        {
                            s.TerminalName = terminals[s.TerminalID].Label;
                            s.MerchantName = terminals[s.TerminalID].BusinessName;
                        }
                    });

                    response.Data = summary;
                    response.NumberOfRecords = numberOfRecords.Value;
                    response.TotalAmount = totalAmount.Value;
                    return Ok(response);
                }
                else
                {
                    // TODO: try to remove ProjectTo
                    response.Data = await mapper.ProjectTo<TransactionSummary>(dataQuery).Future().ToListAsync();
                    response.NumberOfRecords = numberOfRecords.Value;
                    response.TotalAmount = totalAmount.Value;
                    return Ok(response);
                }
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
                    throw new BusinessException(Transactions.Shared.Messages.WhenSpecifiedTokenCCDIsNotValid);
                }

                if (model.DealDetails.ConsumerID == null)
                {
                    model.DealDetails.ConsumerID = await CreateConsumer(model, merchantID.Value);
                }

                // TODO: what if consumer does not created

                if (model.DealDetails.ConsumerID != null)
                {
                    var tokenRequest = mapper.Map<TokenRequest>(model.CreditCardSecureDetails);
                    mapper.Map(model, tokenRequest);

                    DocumentOriginEnum origin = GetDocumentOrigin(null, null, model.PinPad.GetValueOrDefault());
                    var tokenResponse = await cardTokenController.CreateTokenInternal(tokenRequest, origin);

                    var tokenResponseOperation = tokenResponse.GetOperationResponse();

                    if (!(tokenResponseOperation?.Status == StatusEnum.Success))
                    {
                        return tokenResponse;
                    }

                    model.CreditCardToken = tokenResponseOperation.EntityUID;
                    model.CreditCardSecureDetails = null;
                }
            }

            if (model.CreditCardToken != null)
            {
                if (model.CreditCardSecureDetails != null)
                {
                    throw new BusinessException(Transactions.Shared.Messages.WhenSpecifiedTokenCCDetailsShouldBeOmitted);
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
            PaymentRequest dbPaymentRequest = null;
            bool isPaymentIntent = false;

            if (prmodel.PaymentRequestID != null)
            {
                dbPaymentRequest = EnsureExists(await paymentRequestsService.GetPaymentRequests().FirstOrDefaultAsync(m => m.PaymentRequestID == prmodel.PaymentRequestID));

                if (dbPaymentRequest.Status == PaymentRequestStatusEnum.Payed || (int)dbPaymentRequest.Status < 0 || dbPaymentRequest.PaymentTransactionID != null)
                {
                    return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.PaymentRequestStatusIsClosed}", StatusEnum.Error, dbPaymentRequest.PaymentRequestID, httpContextAccessor.TraceIdentifier));
                }
            }
            else
            {
                dbPaymentRequest = EnsureExists(await paymentIntentService.GetPaymentIntent(prmodel.PaymentIntentID.GetValueOrDefault()), "PaymentIntent");

                isPaymentIntent = true;
            }

            // TODO: get from terminal
            var merchantID = dbPaymentRequest.MerchantID ?? User.GetMerchantID();
            CreateTransactionRequest model = new CreateTransactionRequest();

            mapper.Map(dbPaymentRequest, model);
            mapper.Map(prmodel, model);

            if (model.SaveCreditCard == true)
            {
                if (model.CreditCardToken != null)
                {
                    throw new BusinessException(Transactions.Shared.Messages.WhenSpecifiedTokenCCDIsNotValid);
                }

                if (merchantID == null)
                {
                    throw new ApplicationException("MerchantID is empty");
                }

                if (model.DealDetails.ConsumerID == null)
                {
                    model.DealDetails.ConsumerID = await CreateConsumer(model, merchantID.Value);
                }

                var tokenRequest = mapper.Map<TokenRequest>(model.CreditCardSecureDetails);
                mapper.Map(model, tokenRequest);

                DocumentOriginEnum origin = isPaymentIntent ? DocumentOriginEnum.Checkout : DocumentOriginEnum.PaymentRequest;
                var tokenResponse = await cardTokenController.CreateTokenInternal(tokenRequest, origin);

                var tokenResponseOperation = tokenResponse.GetOperationResponse();

                // if TransactionAmount is null/zero  we only create customer & save card, no transaction needed
                if (tokenResponseOperation?.Status == StatusEnum.Success)
                {
                    if (model.TransactionAmount == 0)
                    {
                        if (isPaymentIntent)
                        {
                            await paymentIntentService.DeletePaymentIntent(prmodel.PaymentIntentID.GetValueOrDefault());
                        }
                        else if (prmodel.PaymentRequestID != null)
                        {
                            await paymentRequestsService.UpdateEntityWithStatus(dbPaymentRequest, PaymentRequestStatusEnum.Payed, paymentTransactionID: prmodel.PaymentRequestID, message: Transactions.Shared.Messages.PaymentRequestPaymentSuccessed);
                        }

                        //If billingDealID is present that means it was renew request, we need to reactivate expired billing deal with new token
                        if (dbPaymentRequest.BillingDealID.HasValue)
                        {
                            return await billingController.UpdateBillingDealToken(dbPaymentRequest.BillingDealID.Value, tokenResponse.Value.EntityUID.Value);
                        }

                        return tokenResponse;
                    }
                }
                else
                {
                    return tokenResponse;
                }

                model.CreditCardToken = tokenResponseOperation.EntityUID;
                model.CreditCardSecureDetails = null;
            }

            ActionResult<OperationResponse> createResult = null;

            if (model.CreditCardToken != null)
            {
                if (model.CreditCardSecureDetails != null)
                {
                    throw new BusinessException(Transactions.Shared.Messages.WhenSpecifiedTokenCCDetailsShouldBeOmitted);
                }

                var token = EnsureExists(await keyValueStorage.Get(model.CreditCardToken.ToString()), "CreditCardToken");

                createResult = await ProcessTransaction(model, token,
                    specialTransactionType: dbPaymentRequest.IsRefund ? SpecialTransactionTypeEnum.Refund : SpecialTransactionTypeEnum.RegularDeal, paymentRequestID: prmodel.PaymentRequestID);
            }
            else
            {
                createResult = await ProcessTransaction(model, null,
                    specialTransactionType: dbPaymentRequest.IsRefund ? SpecialTransactionTypeEnum.Refund : SpecialTransactionTypeEnum.RegularDeal, paymentRequestID: prmodel.PaymentRequestID);
            }

            var opResult = createResult.GetOperationResponse();

            if (!(opResult?.Status == StatusEnum.Success))
            {
                //ECNG-442: Payments request. Not possible to make a transaction from Checkout deal in case of SHVA error
                //await paymentRequestsService.UpdateEntityWithStatus(dbPaymentRequest, PaymentRequestStatusEnum.PaymentFailed, paymentTransactionID: opResult?.EntityUID, message: Messages.PaymentRequestPaymentFailed);
            }
            else
            {
                if (isPaymentIntent)
                {
                    await paymentIntentService.DeletePaymentIntent(prmodel.PaymentIntentID.GetValueOrDefault());
                }
                else
                {
                    await paymentRequestsService.UpdateEntityWithStatus(dbPaymentRequest, PaymentRequestStatusEnum.Payed, paymentTransactionID: opResult?.EntityUID, message: Transactions.Shared.Messages.PaymentRequestPaymentSuccessed);
                }
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
            CreditCardTokenKeyVault token;

            // Does it have sense to use J5 together with Token?
            if (!string.IsNullOrWhiteSpace(model.CreditCardToken))
            {
                token = EnsureExists(await keyValueStorage.Get(model.CreditCardToken), "CreditCardToken");
            }
            else
            {
                var tokenRequest = mapper.Map<TokenRequest>(model.CreditCardSecureDetails);
                mapper.Map(model, tokenRequest);
                var dbData = mapper.Map<CreditCardTokenDetails>(tokenRequest);
                var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));
                dbData.MerchantID = terminal.MerchantID;
                DocumentOriginEnum origin = User.IsTerminal() ? DocumentOriginEnum.API : DocumentOriginEnum.UI;

                var tokenResponse = await cardTokenController.CreateTokenInternal(tokenRequest, origin);

                var tokenResponseOperation = tokenResponse.GetOperationResponse();

                if (!(tokenResponseOperation?.Status == StatusEnum.Success))
                {
                    return tokenResponse;
                }

                await creditCardTokenService.CreateEntity(dbData);
                token = EnsureExists(await keyValueStorage.Get(tokenResponseOperation.EntityUID.ToString()), "CreditCardToken");
                transaction.CreditCardSecureDetails = null;
                transaction.CreditCardToken = tokenResponseOperation.EntityUID;
            }

            return await ProcessTransaction(transaction, token, JDealTypeEnum.J5);
        }

        /// <summary>
        /// Implement J5 deal
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("selectJ5/{transactionID}")]
        [ValidateModelState]
        public async Task<ActionResult<OperationResponse>> SelectJ5(Guid? transactionID)
        {
            var transaction = EnsureExists(await transactionsService.GetTransaction(t => t.PaymentTransactionID == transactionID));
            var terminal = EnsureExists(await terminalsService.GetTerminal(transaction.TerminalID));

            var createTransactionReq = mapper.Map<CreateTransactionRequest>(transaction);
            if (transaction.Status != TransactionStatusEnum.AwaitingForSelectJ5)
            {
                return BadRequest(Messages.TransactionStatusIsNotValid);
            }

            TransactionTerminalSettingsValidator.Validate(terminal.Settings, transaction.TransactionDate.Value);

            var token = EnsureExists(await keyValueStorage.Get(createTransactionReq.CreditCardToken.ToString()), "CreditCardToken");
            var response = await ProcessTransaction(createTransactionReq, token, specialTransactionType: SpecialTransactionTypeEnum.RegularDeal);

            return response;
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

            CreditCardTokenKeyVault token = null;

            if (model.CreditCardToken != null)
            {
                token = EnsureExists(await keyValueStorage.Get(model.CreditCardToken), "CreditCardToken");
            }

            return await ProcessTransaction(transaction, token, JDealTypeEnum.J2);
        }

        /// <summary>
        /// Refund request
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrManagerOrAdmin)]
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

        /// <summary>
        /// Method used internally to generate transactions based on prepared billing deals
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Policy = Policy.AnyAdmin)]
        [HttpPost]
        [Route("process-billing-deals")]
        public async Task<ActionResult<CreateTransactionFromBillingDealsResponse>> CreateTransactionsFromBillingDeals(CreateTransactionFromBillingDealsRequest request)
        {
            if (request.BillingDealsID == null || request.BillingDealsID.Count() == 0)
            {
                return BadRequest(new OperationResponse(Transactions.Shared.Messages.BillingDealsRequired, null, HttpContext.TraceIdentifier, nameof(request.BillingDealsID), Transactions.Shared.Messages.BillingDealsRequired));
            }

            if (request.BillingDealsID.Count() > appSettings.BillingDealsMaxBatchSize)
            {
                return BadRequest(new OperationResponse(string.Format(Messages.BillingDealsMaxBatchSize, appSettings.BillingDealsMaxBatchSize), null, httpContextAccessor.TraceIdentifier, nameof(request.BillingDealsID), string.Format(Messages.TransmissionLimit, appSettings.BillingDealsMaxBatchSize)));
            }

            var response = new CreateTransactionFromBillingDealsResponse
            {
                Status = StatusEnum.Success,
                Message = Messages.TransactionsQueued,
                SuccessfulCount = 0,
                FailedCount = 0
            };

            foreach (var billingId in request.BillingDealsID)
            {
                var billingDeal = await billingDealService.GetBillingDealsForUpdate().FirstOrDefaultAsync(d => d.BillingDealID == billingId);
                if (billingDeal == null)
                {
                    logger.LogError($"Billing deal not found: {billingId}");
                    response.FailedCount++;
                    continue;
                }

                // Billing should be prepared before
                if (billingDeal.InProgress != BillingProcessingStatusEnum.Started)
                {
                    logger.LogWarning($"Billing deal transaction generation should be in-progress: {billingDeal.BillingDealID}");
                    response.FailedCount++;
                    continue;
                }

                try
                {
                    billingDeal.InProgress = BillingProcessingStatusEnum.InProgress;
                    await billingDealService.UpdateEntity(billingDeal);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    logger.LogWarning(ex, $"Billing deal transaction generation already be in-progress: {billingDeal.BillingDealID}: {ex.Message}");
                    response.FailedCount++;
                    continue;
                }

                try
                {
                    OperationResponse operationResult = new OperationResponse { Status = StatusEnum.Success };

                    if (!billingDeal.Active)
                    {
                        logger.LogError($"Billing deal is closed: {billingDeal.BillingDealID}");
                        operationResult.Message = $"Billing deal is closed";
                        operationResult.Status = StatusEnum.Error;
                    }

                    var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;

                    var actualDeal = (billingDeal.NextScheduledTransaction != null && billingDeal.NextScheduledTransaction.Value.Date <= today) &&
                        (billingDeal.PausedFrom == null || billingDeal.PausedFrom > today) && (billingDeal.PausedTo == null || billingDeal.PausedTo < today);

                    if (!actualDeal)
                    {
                        logger.LogError($"Billing deal is not actual: {billingDeal.BillingDealID}, NextScheduledTransaction: {billingDeal.NextScheduledTransaction}, PausedFrom: {billingDeal.PausedFrom}, PausedTo: {billingDeal.PausedTo}");
                        operationResult.Message = $"Billing deal is not actual: NextScheduledTransaction: {billingDeal.NextScheduledTransaction}, PausedFrom: {billingDeal.PausedFrom}, PausedTo: {billingDeal.PausedTo}";
                        operationResult.Status = StatusEnum.Error;
                    }

                    CreditCardTokenKeyVault token = null;

                    if (billingDeal.PaymentType == PaymentTypeEnum.Card && billingDeal.InvoiceOnly == false)
                    {
                        token = await keyValueStorage.Get(billingDeal.CreditCardToken.ToString());
                        if (token == null)
                        {
                            logger.LogError($"Credit card token {billingDeal.CreditCardToken} does not exist. Billing deal: {billingDeal.BillingDealID}");
                            operationResult.Status = StatusEnum.Error;
                            operationResult.Message = $"Credit card token {billingDeal.CreditCardToken} does not exist";
                        }
                        else
                        {
                            if (token.CardExpiration.Expired == true)
                            {
                                logger.LogError($"Credit card token {billingDeal.CreditCardToken} expired. Billing deal: {billingDeal.BillingDealID}");
                                operationResult.Status = StatusEnum.Error;
                                operationResult.Message = $"Credit card token {billingDeal.CreditCardToken} expired";
                            }
                        }
                    }

                    if (operationResult.Status == StatusEnum.Success)
                    {
                        operationResult = await NextBillingDeal(billingDeal, token);
                    }

                    if (operationResult.Status == StatusEnum.Success)
                    {
                        response.SuccessfulCount++;
                    }
                    else
                    {
                        billingDeal.InProgress = BillingProcessingStatusEnum.Pending;
                        billingDeal.Active = false;
                        billingDeal.HasError = true;
                        billingDeal.LastError = operationResult.Message;
                        billingDeal.LastErrorCorrelationID = GetCorrelationID();
                        await billingDealService.UpdateEntity(billingDeal);

                        response.FailedCount++;
                    }
                }
                catch (Exception ex)
                {
                    response.FailedCount++;
                    logger.LogError(ex, $"Failed to create transaction for billing deal {billingDeal.BillingDealID}: {ex.Message}");
                }
            }

            if (response.FailedCount > 0 && response.SuccessfulCount == 0)
            {
                response.Status = StatusEnum.Error;
            }
            else if (response.FailedCount > 0)
            {
                response.Status = StatusEnum.Warning;
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("send-transaction-slip-email")]
        public async Task<ActionResult<OperationResponse>> SendTransactionSlipEmail(SendTransactionSlipEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var transaction = EnsureExists(await transactionsService.GetTransaction(t => t.PaymentTransactionID == request.TransactionID));

            var terminal = EnsureExists(await terminalsService.GetTerminal(transaction.TerminalID));

            var response = new OperationResponse
            {
                Status = StatusEnum.Success,
                Message = Messages.EmailSent
            };

            // replace the data with required parameters for one time use, do not save
            transaction.DealDetails.ConsumerEmail = request.Email;
            terminal.Settings.SendTransactionSlipEmailToMerchant = false;

            await SendTransactionSuccessEmails(transaction, terminal);

            return Ok(response);
        }
    }
}