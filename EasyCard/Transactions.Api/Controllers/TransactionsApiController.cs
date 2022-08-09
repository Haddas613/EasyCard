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
using Ecwid.Api;
using Shared.Helpers.Events;
using Merchants.Business.Extensions;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Helpers.IO;
using SharedApi = Shared.Api;
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
        private readonly ISystemSettingsService systemSettingsService;
        private readonly IMerchantsService merchantsService;
        private readonly IEmailSender emailSender;
        private readonly IEventsService events;
        private readonly IPaymentIntentService paymentIntentService;
        private readonly InvoicingController invoicingController;
        private readonly BasicServices.Services.IExcelService excelService;
        private readonly IHubContext<Hubs.TransactionsHub, Shared.Hubs.ITransactionsHub> transactionsHubContext;
        private readonly IEcwidApiClient ecwidApiClient;
        private readonly External.BitApiController bitController;
        private readonly IThreeDSIntermediateStorage threeDSIntermediateStorage;
        private readonly TransmissionController transmissionController;

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
            IPaymentRequestsService paymentRequestsService,
            IHttpContextAccessorWrapper httpContextAccessor,
            ISystemSettingsService systemSettingsService,
            IMerchantsService merchantsService,
            IEmailSender emailSender,
            IOptions<ApiSettings> apiSettings,
            IEventsService events,
            IPaymentIntentService paymentIntentService,
            IHubContext<Hubs.TransactionsHub, Shared.Hubs.ITransactionsHub> transactionsHubContext,
            BillingController billingController,
            InvoicingController invoicingController,
            BasicServices.Services.IExcelService excelService,
            IEcwidApiClient ecwidApiClient,
            External.BitApiController bitController,
            IThreeDSIntermediateStorage threeDSIntermediateStorage,
            TransmissionController transmissionController)
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
            this.paymentRequestsService = paymentRequestsService;
            this.httpContextAccessor = httpContextAccessor;
            this.systemSettingsService = systemSettingsService;
            this.merchantsService = merchantsService;
            this.emailSender = emailSender;
            this.events = events;
            this.paymentIntentService = paymentIntentService;
            this.transactionsHubContext = transactionsHubContext;
            this.invoicingController = invoicingController;
            this.excelService = excelService;
            this.ecwidApiClient = ecwidApiClient;
            this.bitController = bitController;
            this.threeDSIntermediateStorage = threeDSIntermediateStorage;
            this.transmissionController = transmissionController;
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
            var tr = EnsureExists(
                await transactionsService.GetTransactions().FirstOrDefaultAsync(m => m.PaymentTransactionID == transactionID));
            var transaction = mapper.Map<TransactionResponse>(tr);

            TransactionStatusExtensions.UpdateAggregatorDetails(tr, transaction);

            var terminal = EnsureExists(await terminalsService.GetTerminal(transaction.TerminalID.Value));

            transaction.TerminalName = terminal.Label;
            transaction.MerchantName = terminal.Merchant.BusinessName;

            IAggregator aggregator = null;

            var terminalAggregator = terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator);

            if (terminalAggregator != null)
            {
                aggregator = aggregatorResolver.GetAggregator(terminalAggregator);
            }

            transaction.AllowTransmission = tr.AllowTransmission();

            transaction.AllowTransmissionCancellation = tr.AllowTransmissionCancellation(aggregator);

            transaction.AllowInvoiceCreation = tr.AllowInvoiceCreation(terminal);

            transaction.AllowRefund = tr.AllowRefund(terminal);

            return Ok(transaction);
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
                var totalAmount = new
                {
                    ILS = query.Where(e => e.Currency == CurrencyEnum.ILS).DeferredSum(e => e.TotalAmount).FutureValue(),
                    USD = query.Where(e => e.Currency == CurrencyEnum.USD).DeferredSum(e => e.TotalAmount).FutureValue(),
                    EUR = query.Where(e => e.Currency == CurrencyEnum.EUR).DeferredSum(e => e.TotalAmount).FutureValue(),
                };

                if (httpContextAccessor.GetUser().IsAdmin())
                {
                    var response = new SummariesAmountResponse<TransactionSummaryAdmin>();
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
                    response.TotalAmountILS = totalAmount.ILS.Value;
                    response.TotalAmountUSD = totalAmount.USD.Value;
                    response.TotalAmountEUR = totalAmount.EUR.Value;

                    return Ok(response);
                }
                else
                {
                    var response = new SummariesAmountResponse<TransactionSummary>();
                    // TODO: try to remove ProjectTo
                    var summary = await mapper.ProjectTo<TransactionSummary>(dataQuery).Future().ToListAsync();

                    var terminalsId = summary.Select(t => t.TerminalID).Distinct();

                    var terminals = await terminalsService.GetTerminals()
                        .Include(t => t.Merchant)
                        .Where(t => terminalsId.Contains(t.TerminalID))
                        .Select(t => new { t.TerminalID, t.Label, t.Merchant.BusinessName })
                        .ToDictionaryAsync(k => k.TerminalID, v => new { v.Label, v.BusinessName });

                    summary.ForEach(s =>
                    {
                        if (terminals.ContainsKey(s.TerminalID))
                        {
                            s.TerminalName = terminals[s.TerminalID].Label;
                        }
                    });

                    response.Data = summary;
                    response.NumberOfRecords = numberOfRecords.Value;
                    response.TotalAmountILS = totalAmount.ILS.Value;
                    response.TotalAmountUSD = totalAmount.USD.Value;
                    response.TotalAmountEUR = totalAmount.EUR.Value;

                    return Ok(response);
                }
            }
        }

        /// <summary>
        /// Get payment transactions list using filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("$excel")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<OperationResponse>> GetTransactionsExcel([FromQuery] TransactionsFilter filter)
        {
            Debug.WriteLine(User);
            var merchantID = User.GetMerchantID();
            var userIsTerminal = User.IsTerminal();

            TransactionsFilterValidator.ValidateFilter(filter, new TransactionFilterValidationOptions { MaximumPageSize = appSettings.FiltersGlobalPageSizeLimit });

            var query = transactionsService.GetTransactions().AsNoTracking().Filter(filter);

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var dataQuery = query.OrderByDynamic(filter.SortBy ?? nameof(PaymentTransaction.PaymentTransactionID), filter.SortDesc);

                if (httpContextAccessor.GetUser().IsAdmin())
                {
                    var summary = await mapper.ProjectTo<TransactionSummaryAdmin>(dataQuery).ToListAsync();

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

                    var mapping = TransactionSummaryResource.ResourceManager.GetExcelColumnNames<TransactionSummaryAdmin>();

                    var terminalLabel = string.Empty;
                    if (filter.TerminalID.HasValue)
                    {
                        var tlabel = terminals
                           .Where(t => t.Key == filter.TerminalID)
                           .Select(t => t.Value)
                           .FirstOrDefault();

                        terminalLabel = $"-{tlabel}";
                    }

                    var filename = FileNameHelpers.RemoveIllegalFilenameCharacters($"Transactions_{Guid.NewGuid()}{terminalLabel}.xlsx");
                    var res = await excelService.GenerateFile($"Admin/{filename}", "Transactions", summary, mapping);

                    return Ok(new OperationResponse { Status = SharedApi.Models.Enums.StatusEnum.Success, EntityReference = res });
                }
                else
                {
                    var data = await mapper.ProjectTo<TransactionSummary>(dataQuery).ToListAsync();
                    var mapping = TransactionSummaryResource.ResourceManager.GetExcelColumnNames<TransactionSummary>();

                    var terminalsId = data.Select(t => t.TerminalID).Distinct();

                    var terminals = await terminalsService.GetTerminals()
                        .Include(t => t.Merchant)
                        .Where(t => terminalsId.Contains(t.TerminalID))
                        .Select(t => new { t.TerminalID, t.Label, t.Merchant.BusinessName })
                        .ToDictionaryAsync(k => k.TerminalID, v => new { v.Label, v.BusinessName });

                    data.ForEach(s =>
                    {
                        if (terminals.ContainsKey(s.TerminalID))
                        {
                            s.TerminalName = terminals[s.TerminalID].Label;
                        }
                    });

                    var terminalLabel = string.Empty;
                    if (filter.TerminalID.HasValue)
                    {
                        var tlabel = terminals
                           .Where(t => t.Key == filter.TerminalID)
                           .Select(t => t.Value)
                           .FirstOrDefault();

                        terminalLabel = $"-{tlabel}";
                    }

                    var filename = FileNameHelpers.RemoveIllegalFilenameCharacters($"Transactions{terminalLabel}.xlsx");
                    var res = await excelService.GenerateFile($"{User.GetMerchantID()}/{filename}", "Transactions", data, mapping);

                    return Ok(new OperationResponse { Status = SharedApi.Models.Enums.StatusEnum.Success, EntityReference = res });
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
            var terminal = await GetTerminal(model.TerminalID);

            if (model.PaymentRequestID != null)
            {
                var query = transactionsService.GetTransactions().AsNoTracking().Filter(new TransactionsFilter { PaymentTransactionRequestID = model.PaymentRequestID });

                using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
                {
                    if (await query.Where(t => (int)t.Status >= (int)TransactionStatusEnum.Initial).CountAsync() > 0)
                    {
                        throw new BusinessException(Messages.PaymentRequestAlreadyPayed);
                    }
                }
            }

            if (model.PaymentIntentID != null)
            {
                var query = transactionsService.GetTransactions().AsNoTracking().Filter(new TransactionsFilter { PaymentTransactionIntentID = model.PaymentIntentID });

                using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
                {
                    if (await query.Where(t => (int)t.Status >= (int)TransactionStatusEnum.Initial).CountAsync() > 0)
                    {
                        throw new BusinessException(Messages.PaymentRequestAlreadyPayed);
                    }
                }
            }

            if (model.PaymentIntentID != null)
            {
                var paymenttransaction = transactionsService.GetTransaction(t => t.PaymentIntentID == model.PaymentIntentID).Result;

                if (paymenttransaction != null && (int)paymenttransaction.Status >= (int)TransactionStatusEnum.Initial)
                {
                    throw new BusinessException(Messages.PaymentRequestAlreadyPayed);
                }
            }

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
            }

            // TODO: what if consumer does not created
            if (model.CreditCardToken == null && model.SaveCreditCard == true && model.CreditCardSecureDetails != null)
            {
                bool doNotCreateInitialDealAndDbRecord = !model.SaveCreditCard.GetValueOrDefault();

                var tokenRequest = mapper.Map<TokenRequest>(model.CreditCardSecureDetails);
                mapper.Map(model, tokenRequest);

                DocumentOriginEnum origin = GetDocumentOrigin(null, null, model.PinPad.GetValueOrDefault());
                var tokenResponse = await cardTokenController.CreateTokenInternal(terminal, tokenRequest, origin, doNotCreateInitialDealAndDbRecord: doNotCreateInitialDealAndDbRecord);

                var tokenResponseOperation = tokenResponse.GetOperationResponse();

                if (!(tokenResponseOperation?.Status == StatusEnum.Success))
                {
                    return tokenResponse;
                }

                model.CreditCardToken = tokenResponseOperation.EntityUID;
                model.CreditCardSecureDetails = null; // TODO
            }

            if (model.CreditCardToken != null)
            {
                if (model.CreditCardSecureDetails != null)
                {
                    throw new BusinessException(Transactions.Shared.Messages.WhenSpecifiedTokenCCDetailsShouldBeOmitted);
                }

                var token = EnsureExists(await keyValueStorage.Get(model.CreditCardToken.ToString()), "CreditCardToken");
                return await ProcessTransaction(terminal, model, token, specialTransactionType: SpecialTransactionTypeEnum.RegularDeal);
            }
            else
            {
                return await ProcessTransaction(terminal, model, null);
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
            if (merchantID == null)
            {
                throw new ApplicationException("MerchantID is empty");
            }

            CreateTransactionRequest model = new CreateTransactionRequest();

            mapper.Map(dbPaymentRequest, model);
            mapper.Map(prmodel, model);

            var terminal = await GetTerminal(model.TerminalID);

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
            }

            if (model.CreditCardToken == null && model.SaveCreditCard == true && model.CreditCardSecureDetails != null)
            {
                bool doNotCreateInitialDealAndDbRecord = !model.SaveCreditCard.GetValueOrDefault();

                var tokenRequest = mapper.Map<TokenRequest>(model.CreditCardSecureDetails);
                mapper.Map(model, tokenRequest);

                DocumentOriginEnum origin = isPaymentIntent ? DocumentOriginEnum.Checkout : DocumentOriginEnum.PaymentRequest;
                var tokenResponse = await cardTokenController.CreateTokenInternal(terminal, tokenRequest, origin, doNotCreateInitialDealAndDbRecord: doNotCreateInitialDealAndDbRecord);

                var tokenResponseOperation = tokenResponse.GetOperationResponse();

                if (!(tokenResponseOperation?.Status == StatusEnum.Success))
                {
                    return tokenResponse;
                }

                // if TransactionAmount is null/zero  we only create customer & save card, no transaction needed
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

                createResult = await ProcessTransaction(terminal, model, token,
                    specialTransactionType: dbPaymentRequest.IsRefund ? SpecialTransactionTypeEnum.Refund : SpecialTransactionTypeEnum.RegularDeal, paymentRequestID: prmodel.PaymentRequestID);
            }
            else
            {
                createResult = await ProcessTransaction(terminal, model, null,
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
            var terminal = await GetTerminal(model.TerminalID);

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
                dbData.MerchantID = terminal.MerchantID;
                DocumentOriginEnum origin = User.IsTerminal() ? DocumentOriginEnum.API : DocumentOriginEnum.UI;

                var tokenResponse = await cardTokenController.CreateTokenInternal(terminal, tokenRequest, origin);

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

            return await ProcessTransaction(terminal, transaction, token, JDealTypeEnum.J5);
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
            var response = await ProcessTransaction(terminal, createTransactionReq, token, specialTransactionType: SpecialTransactionTypeEnum.RegularDeal);

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
            var terminal = await GetTerminal(model.TerminalID);
            CreditCardTokenKeyVault token = null;

            if (model.CreditCardToken != null)
            {
                token = EnsureExists(await keyValueStorage.Get(model.CreditCardToken), "CreditCardToken");
            }

            return await ProcessTransaction(terminal, transaction, token, JDealTypeEnum.J2);
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
            var terminal = await GetTerminal(model.TerminalID);
            if (!string.IsNullOrWhiteSpace(model.CreditCardToken))
            {
                var token = EnsureExists(await keyValueStorage.Get(model.CreditCardToken), "CreditCardToken");
                return await ProcessTransaction(terminal, transaction, token, JDealTypeEnum.J4, SpecialTransactionTypeEnum.Refund);
            }
            else
            {
                return await ProcessTransaction(terminal, transaction, null, JDealTypeEnum.J4, SpecialTransactionTypeEnum.Refund);
            }
        }

        /// <summary>
        /// Refund or chargeback of and existing transaction
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrManagerOrAdmin)]
        [Route("chargeback")]
        [ValidateModelState]
        public async Task<ActionResult<OperationResponse>> Chargeback([FromBody] ChargebackRequest request)
        {
            var transaction = EnsureExists(
                await transactionsService.GetTransactionsForUpdate().FirstOrDefaultAsync(m => m.PaymentTransactionID == request.ExistingPaymentTransactionID));

            var terminal = EnsureExists(await terminalsService.GetTerminal(transaction.TerminalID));

            IAggregator aggregator = null;

            var terminalAggregator = terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator);

            if (terminalAggregator != null)
            {
                aggregator = aggregatorResolver.GetAggregator(terminalAggregator);
            }

            if (transaction.AllowTransmissionCancellation(aggregator))
            {
                if (transaction.TransactionAmount != request.RefundAmount)
                {
                    return new OperationResponse($"It is not possible to cancel transaction {transaction.PaymentTransactionID} for amount {request.RefundAmount}", StatusEnum.Error);
                }

                object aggregatorSettings = null;

                if (aggregator != null)
                {
                    aggregatorSettings = aggregatorResolver.GetAggregatorTerminalSettings(terminalAggregator, terminalAggregator.Settings);
                }

                // TODO: db-transaction
                return await transmissionController.CancelNotTransmittedTransactionInternal(terminal, transaction, aggregator, aggregatorSettings);
            }

            if (!transaction.AllowRefund(terminal))
            {
                return new OperationResponse($"It is not possible to make refund for transaction {transaction.PaymentTransactionID}", StatusEnum.Error);
            }

            if (transaction.DocumentOrigin == DocumentOriginEnum.Bit)
            {
                var res = await bitController.RefundInternal(transaction, terminal, request);

                if (res.Status == StatusEnum.Success)
                {
                    _ = SendTransactionSuccessEmails(transaction, terminal);

                    return res;
                }
                else
                {
                    return BadRequest(res);
                }
            }
            else
            {
                var transactionRequest = ConvertTransactionRequestForRefund(transaction, request.RefundAmount);

                if (transaction.CreditCardToken != null)
                {
                    var token = EnsureExists(await keyValueStorage.Get(transaction.CreditCardToken.ToString()), "CreditCardToken");

                    return await ProcessTransaction(terminal, transactionRequest, token, JDealTypeEnum.J4, SpecialTransactionTypeEnum.Refund,
                        initialTransactionID: transaction.PaymentTransactionID,
                        initialTransactionProcess: async (IDbContextTransaction dbTransaction) =>
                        {
                            transaction.TotalRefund = transaction.TotalRefund.GetValueOrDefault() + request.RefundAmount;

                            await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.Chargeback, transactionOperationCode: TransactionOperationCodesEnum.RefundCreated, dbTransaction: dbTransaction);
                        },
                        compensationTransactionProcess: async (IDbContextTransaction dbTransaction) =>
                        {
                            transaction.TotalRefund = transaction.TotalRefund.GetValueOrDefault() - request.RefundAmount;

                            await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.ChargebackFailed, transactionOperationCode: TransactionOperationCodesEnum.RefundFailed, dbTransaction: dbTransaction);
                        }
                        );
                }
                else
                {
                    throw new BusinessException($"Saved Credit Card required");
                }
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
                var billingDeal = await billingDealService.GetBillingDeal(billingId);
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
                    Terminal terminal = await GetTerminal(billingDeal.TerminalID);
                    OperationResponse operationResult = await CreateTransactionsFromBillingDealInternal(billingDeal, terminal);

                    if (operationResult.Status == StatusEnum.Success)
                    {
                        response.SuccessfulCount++;
                    }
                    else
                    {
                        billingDeal.UpdateNextScheduledDatAfterError(operationResult.Message, GetCorrelationID(), terminal.BillingSettings.FailedTransactionsCountBeforeInactivate, terminal.BillingSettings.NumberOfDaysToRetryTransaction);

                        await billingDealService.UpdateEntityWithHistory(billingDeal, Messages.TriggerTransactionFailed, BillingDealOperationCodesEnum.TriggerTransactionFailed);

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

        /// <summary>
        /// send Shovar
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
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
            terminal.Settings.SendTransactionSlipEmailToConsumer = true;
            terminal.Settings.SendTransactionSlipEmailToMerchant = false;

            _ = SendTransactionSuccessEmails(transaction, terminal);

            return Ok(response);
        }

        private async Task<OperationResponse> CreateTransactionsFromBillingDealInternal(BillingDeal billingDeal, Terminal terminal)
        {
            var operationResult = new OperationResponse { Status = StatusEnum.Success };
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
                // NOTE: this is admin-scoped method
                var tokenData = await creditCardTokenService.GetTokens().Where(d => d.CreditCardTokenID == billingDeal.CreditCardToken).FirstOrDefaultAsync();
                if (tokenData == null)
                {
                    logger.LogError($"Credit card token {billingDeal.CreditCardToken} does not exist. Billing deal: {billingDeal.BillingDealID}");
                    operationResult.Status = StatusEnum.Error;
                    operationResult.Message = $"Credit card token {billingDeal.CreditCardToken} does not exist";

                    return operationResult;
                }

                if (tokenData.CardExpiration.Expired == true)
                {
                    logger.LogError($"Credit card token {billingDeal.CreditCardToken} expired. Billing deal: {billingDeal.BillingDealID}");
                    operationResult.Status = StatusEnum.Error;
                    operationResult.Message = $"Credit card token {billingDeal.CreditCardToken} expired";

                    return operationResult;
                }

                token = await keyValueStorage.Get(billingDeal.CreditCardToken.ToString());
                if (token == null)
                {
                    logger.LogError($"Credit card token {billingDeal.CreditCardToken} does not exist. Billing deal: {billingDeal.BillingDealID}");
                    operationResult.Status = StatusEnum.Error;
                    operationResult.Message = $"Credit card token {billingDeal.CreditCardToken} does not exist";

                    return operationResult;
                }

                if (token.CardExpiration.Expired == true)
                {
                    logger.LogError($"Credit card token {billingDeal.CreditCardToken} expired. Billing deal: {billingDeal.BillingDealID}");
                    operationResult.Status = StatusEnum.Error;
                    operationResult.Message = $"Credit card token {billingDeal.CreditCardToken} expired";

                    return operationResult;
                }
            }

            return await NextBillingDeal(terminal, billingDeal, token);
        }
    }
}