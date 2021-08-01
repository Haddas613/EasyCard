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
        private readonly ApiSettings apiSettings;
        private readonly ICreditCardTokenService creditCardTokenService;
        private readonly IBillingDealService billingDealService;
        private readonly ITerminalsService terminalsService;
        private readonly IConsumersService consumersService;
        private readonly CardTokenController cardTokenController;
        private readonly IPaymentRequestsService paymentRequestsService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly IInvoiceService invoiceService;
        private readonly ISystemSettingsService systemSettingsService;
        private readonly IMerchantsService merchantsService;
        private readonly IQueue invoiceQueue;
        private readonly IEmailSender emailSender;
        private readonly IMetricsService metrics;
        private readonly IPaymentIntentService paymentIntentService;

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
            IPaymentIntentService paymentIntentService)
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
            this.invoiceService = invoiceService;
            this.paymentRequestsService = paymentRequestsService;
            this.httpContextAccessor = httpContextAccessor;
            this.systemSettingsService = systemSettingsService;
            this.merchantsService = merchantsService;
            this.invoiceQueue = queueResolver.GetQueue(QueueResolver.InvoiceQueue);
            this.emailSender = emailSender;
            this.metrics = metrics;
            this.paymentIntentService = paymentIntentService;
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

        [HttpGet]
        [Route("{transactionID}")]
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

                //TODO: cache
                var merchantName = await merchantsService.GetMerchants().Where(m => m.MerchantID == transaction.MerchantID).Select(m => m.BusinessName).FirstOrDefaultAsync();
                if (transaction.TerminalID.HasValue)
                {
                    transaction.TerminalName = await terminalsService.GetTerminals().Where(t => t.TerminalID == transaction.TerminalID.Value).Select(t => t.Label).FirstOrDefaultAsync();
                }

                transaction.MerchantName = merchantName;
                return Ok(transaction);
            }
            else
            {
                var transaction = mapper.Map<TransactionResponse>(EnsureExists(
                    await transactionsService.GetTransactions().FirstOrDefaultAsync(m => m.PaymentTransactionID == transactionID)));

                if (transaction.TerminalID.HasValue)
                {
                    transaction.TerminalName = await terminalsService.GetTerminals().Where(t => t.TerminalID == transaction.TerminalID.Value).Select(t => t.Label).FirstOrDefaultAsync();
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
                var dataQuery = query.OrderByDynamic(filter.SortBy ?? nameof(PaymentTransaction.PaymentTransactionID), filter.SortDesc).ApplyPagination(filter, appSettings.FiltersGlobalPageSizeLimit);

                var numberOfRecords = query.DeferredCount().FutureValue();
                var response = new SummariesResponse<TransactionSummary>();

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
                    return Ok(response);
                }
                else
                {
                    // TODO: try to remove ProjectTo
                    response.Data = await mapper.ProjectTo<TransactionSummary>(dataQuery).Future().ToListAsync();
                    response.NumberOfRecords = numberOfRecords.Value;
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
                dbPaymentRequest = EnsureExists(await paymentIntentService.GetPaymentIntent(prmodel.TerminalID, prmodel.PaymentIntentID.GetValueOrDefault()), "PaymentIntent");

                isPaymentIntent = true;
            }

            CreateTransactionRequest model = new CreateTransactionRequest();

            mapper.Map(dbPaymentRequest, model);
            mapper.Map(prmodel, model);

            if (model.SaveCreditCard == true)
            {
                if (model.CreditCardToken != null)
                {
                    throw new BusinessException(Transactions.Shared.Messages.WhenSpecifiedTokenCCDIsNotValid);
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
                    await paymentIntentService.DeletePaymentIntent(prmodel.TerminalID, prmodel.PaymentIntentID.GetValueOrDefault());
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

        [HttpPost]
        [Route("trigger-billing-deals")]
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
                var billingDeal = EnsureExists(await billingDealService.GetBillingDealsForUpdate().FirstOrDefaultAsync(d => d.BillingDealID == billingId));

                if (!billingDeal.Active)
                {
                    logger.LogError($"Billing deal is closed: {billingDeal.BillingDealID}");
                    response.FailedCount++;
                    continue;
                }

                var operationResult = await NextBillingDeal(billingDeal);

                if (operationResult.Status == StatusEnum.Success)
                {
                    response.SuccessfulCount++;
                }
                else
                {
                    response.FailedCount++;
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
        public async Task<ActionResult<OperationResponse>> CreateTransactionsFromBillingDeals(SendTransactionSlipEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var transaction = EnsureExists(await transactionsService.GetTransaction(t => t.PaymentTransactionID == request.TransactionID));

            if (transaction.Status != TransactionStatusEnum.Completed)
            {
                return BadRequest(Messages.TransactionMustBeCompleted);
            }

            if (transaction.JDealType != JDealTypeEnum.J4)
            {
                return BadRequest(transaction.JDealType == JDealTypeEnum.J2 ? Messages.J2NotAllowed : Messages.J5NotAllowed);
            }

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

        private async Task<ActionResult<OperationResponse>> ProcessTransaction(CreateTransactionRequest model, CreditCardTokenKeyVault token, JDealTypeEnum jDealType = JDealTypeEnum.J4, SpecialTransactionTypeEnum specialTransactionType = SpecialTransactionTypeEnum.RegularDeal, Guid? initialTransactionID = null, BillingDeal billingDeal = null, Guid? paymentRequestID = null)
        {
            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            if (model.PinPad == true && string.IsNullOrWhiteSpace(model.PinPadDeviceID))
            {
                var nayaxIntegration = EnsureExists(terminal.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.NayaxPinpadProcessorExternalSystemID));
                var devices = nayaxIntegration.Settings.ToObject<Nayax.NayaxTerminalCollection>();
                var firstDevice = devices.devices.FirstOrDefault();

                if (firstDevice == null)
                {
                    throw new EntityNotFoundException(SharedBusiness.Messages.ApiMessages.EntityNotFound, "PinPadDevice", null);
                }

                model.PinPadDeviceID = firstDevice.TerminalID;
            }

            TransactionTerminalSettingsValidator.Validate(terminal.Settings, model, token, jDealType, specialTransactionType, initialTransactionID);

            var transaction = mapper.Map<PaymentTransaction>(model);

            // NOTE: this is security assignment
            mapper.Map(terminal, transaction);

            bool pinpadDeal = model.PinPad ?? false;

            transaction.SpecialTransactionType = specialTransactionType;
            transaction.JDealType = jDealType;
            transaction.BillingDealID = billingDeal?.BillingDealID;
            transaction.InitialTransactionID = initialTransactionID;
            transaction.DocumentOrigin = GetDocumentOrigin(billingDeal?.BillingDealID, paymentRequestID, pinpadDeal);
            transaction.PaymentRequestID = paymentRequestID;
            transaction.CardPresence = pinpadDeal ? CardPresenceEnum.Regular : model.CardPresence;

            if (transaction.DealDetails == null)
            {
                transaction.DealDetails = new Business.Entities.DealDetails();
            }

            if (model.IssueInvoice == true && model.InvoiceDetails == null)
            {
                //TODO: Handle refund & credit default invoice types
                model.InvoiceDetails = new SharedIntegration.Models.Invoicing.InvoiceDetails { InvoiceType = terminal.InvoiceSettings.DefaultInvoiceType.GetValueOrDefault() };
            }

            // Update card information based on token
            CreditCardTokenDetails dbToken = null;

            //TODO: check token expiration
            if (token != null)
            {
                if (token.TerminalID != terminal.TerminalID)
                {
                    throw new EntityNotFoundException(SharedBusiness.Messages.ApiMessages.EntityNotFound, "CreditCardToken", null);
                }

                if (token.CardExpiration?.Expired == true)
                {
                    return BadRequest(new OperationResponse($"{Messages.CreditCardExpired}", StatusEnum.Error, model.CreditCardToken));
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
                        throw new EntityConflictException(Transactions.Shared.Messages.ConsumerNatIdIsNotEqTranNatId, "Consumer");
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(consumer.ConsumerNationalID) && model.CreditCardSecureDetails != null && !string.IsNullOrWhiteSpace(model.CreditCardSecureDetails.CardOwnerNationalID) && !consumer.ConsumerNationalID.Equals(model.CreditCardSecureDetails.CardOwnerNationalID, StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new EntityConflictException(Transactions.Shared.Messages.ConsumerNatIdIsNotEqTranNatId, "Consumer");
                    }
                }
            }

            // Update details if needed
            transaction.DealDetails.UpdateDealDetails(consumer, terminal.Settings, transaction);
            if (model.IssueInvoice.GetValueOrDefault())
            {
                model.InvoiceDetails.UpdateInvoiceDetails(terminal.InvoiceSettings, transaction);
            }

            if (string.IsNullOrWhiteSpace(transaction.CreditCardDetails.CardOwnerName) && consumer != null)
            {
                transaction.CreditCardDetails.CardOwnerName = consumer.ConsumerName;
            }

            transaction.Calculate();

            transaction.MerchantIP = GetIP();
            transaction.CorrelationId = GetCorrelationID();

            var terminalAggregator = ValidateExists(
                terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator),
                Transactions.Shared.Messages.AggregatorNotDefined);

            var terminalProcessor = ValidateExists(
                terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Processor),
                Transactions.Shared.Messages.ProcessorNotDefined);

            TerminalExternalSystem terminalPinpadProcessor = null;

            if (pinpadDeal)
            {
                terminalPinpadProcessor = ValidateExists(
                    terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.PinpadProcessor),
                    Transactions.Shared.Messages.ProcessorNotDefined);
            }

            transaction.AggregatorID = terminalAggregator.ExternalSystemID;
            transaction.ProcessorID = terminalProcessor.ExternalSystemID;

            var aggregator = aggregatorResolver.GetAggregator(terminalAggregator);
            var processor = processorResolver.GetProcessor(terminalProcessor);
            IProcessor pinpadProcessor = null;
            if (pinpadDeal)
            {
                pinpadProcessor = processorResolver.GetProcessor(terminalPinpadProcessor);
            }

            var aggregatorSettings = aggregatorResolver.GetAggregatorTerminalSettings(terminalAggregator, terminalAggregator.Settings);
            mapper.Map(aggregatorSettings, transaction);

            var processorSettings = processorResolver.GetProcessorTerminalSettings(terminalProcessor, terminalProcessor.Settings);
            mapper.Map(processorSettings, transaction);

            object pinpadProcessorSettings = null;
            if (pinpadDeal)
            {
                if (terminalPinpadProcessor?.Settings != null && terminalPinpadProcessor.ExternalSystemID == ExternalSystemHelpers.NayaxPinpadProcessorExternalSystemID)
                {
                    var devices = terminalPinpadProcessor.Settings.GetValue("devices")?.ToObject<IEnumerable<JObject>>();
                    if (devices != null)
                    {
                        terminalPinpadProcessor.Settings = EnsureExists(
                            devices.FirstOrDefault(d => d.GetValue("terminalID").Value<string>() == model.PinPadDeviceID), "PinPad Terminal");
                    }
                }

                pinpadProcessorSettings = processorResolver.GetProcessorTerminalSettings(terminalPinpadProcessor, terminalPinpadProcessor.Settings);
                mapper.Map(pinpadProcessorSettings, transaction);
            }

            await transactionsService.CreateEntity(transaction);
            metrics.TrackTransactionEvent(transaction, TransactionOperationCodesEnum.TransactionCreated);

            ProcessorPreCreateTransactionResponse pinpadPreCreateResult = null;
            var processorRequest = mapper.Map<ProcessorCreateTransactionRequest>(transaction);
            if (pinpadDeal)
            {
                try
                {
                    processorRequest.PinPadProcessorSettings = pinpadProcessorSettings;
                    var lastDeal = await GetLastShvaTransactionDetails(transaction.ShvaTransactionDetails.ShvaTerminalID);
                    mapper.Map(lastDeal, processorRequest); // Map details of prev shva transaction

                    pinpadPreCreateResult = await pinpadProcessor.PreCreateTransaction(processorRequest);
                    if (!pinpadPreCreateResult.Success)
                    {
                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToConfirmByProcesor, rejectionMessage: pinpadPreCreateResult.ErrorMessage);

                        return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.RejectedByProcessor}: {pinpadPreCreateResult.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, pinpadPreCreateResult.Errors));
                    }
                    else
                    {
                        mapper.Map(pinpadPreCreateResult, processorRequest);

                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.ConfirmedByPinpadPreProcessor);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Pinpad Processor PreCreate Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToConfirmByProcesor, rejectionMessage: ex.Message);

                    return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, pinpadPreCreateResult?.Errors));
                }
            }

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

            BadRequestObjectResult processorFailedRsponse = null;

            // create transaction in processor (Shva)
            try
            {
                mapper.Map(transaction, processorRequest);

                if (!pinpadDeal)
                {
                    if (token != null)
                    {
                        mapper.Map(token, processorRequest.CreditCardToken);
                        mapper.Map(dbToken.ShvaInitialTransactionDetails, processorRequest.InitialDeal, typeof(ShvaInitialTransactionDetails), typeof(Shva.Models.InitDealResultModel)); // TODO: remove direct Shva reference
                    }
                    else
                    {
                        mapper.Map(model.CreditCardSecureDetails, processorRequest.CreditCardToken);
                    }
                }

                processorRequest.ProcessorSettings = processorSettings;

                var processorResponse = pinpadDeal ? await pinpadProcessor.CreateTransaction(processorRequest) : await processor.CreateTransaction(processorRequest);

                mapper.Map(processorResponse, transaction);

                if (pinpadDeal)
                {
                    await transactionsService.Refresh(transaction);
                }

                if (!processorResponse.Success)
                {
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedByProcessor, TransactionFinalizationStatusEnum.Initial, rejectionMessage: processorResponse.ErrorMessage, rejectionReason: processorResponse.RejectReasonCode);

                    if (processorResponse.RejectReasonCode == RejectionReasonEnum.AuthorizationCodeRequired)
                    {
                        var message = Messages.AuthorizationCodeRequired.Replace("@number", processorResponse.TelToGetAuthNum).Replace("@retailer", processorResponse.CompRetailerNum);
                        processorFailedRsponse = BadRequest(new OperationResponse(message, StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier));
                    }
                    else
                    {
                        processorFailedRsponse = BadRequest(new OperationResponse($"{Transactions.Shared.Messages.RejectedByProcessor}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, processorResponse.Errors));
                    }

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

                processorFailedRsponse = BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionStatusEnum.FailedToConfirmByProcesor.ToString(), (ex as IntegrationException)?.Message));
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

                            return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}: {aggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier));
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

                        return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionFinalizationStatusEnum.FailedToCancelByAggregator.ToString(), (ex as IntegrationException)?.Message));
                    }
                }

                // commit transaction in aggregator (Clearing House or upay)
                else
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
                            await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.AwaitingForTransmission, transactionOperationCode: TransactionOperationCodesEnum.CommitedByAggregator);
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
            else if (processorFailedRsponse != null)
            {
                return processorFailedRsponse;
            }
            else
            {
                if (jDealType != JDealTypeEnum.J4)
                {
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.Completed);
                }
                else
                {
                    //If aggregator is not required transaction is eligible for transmission
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.AwaitingForTransmission);
                }
            }

            if (billingDeal != null && jDealType == JDealTypeEnum.J4)
            {
                billingDeal.CurrentDeal = billingDeal.CurrentDeal.HasValue ? billingDeal.CurrentDeal.Value + 1 : 1;
                billingDeal.NextScheduledTransaction = billingDeal.BillingSchedule?.GetNextScheduledDate(transaction.TransactionDate.Value, billingDeal.CurrentDeal.Value);
                billingDeal.Active = billingDeal.NextScheduledTransaction != null;

                await billingDealService.UpdateEntity(billingDeal);
            }

            var endResponse = new OperationResponse(Transactions.Shared.Messages.TransactionCreated, StatusEnum.Success, transaction.PaymentTransactionID);

            // TODO: validate InvoiceDetails
            if (model.IssueInvoice == true && !string.IsNullOrWhiteSpace(transaction.DealDetails.ConsumerEmail) && model.Currency == CurrencyEnum.ILS)
            {
                using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
                {
                    try
                    {
                        Invoice invoiceRequest = new Invoice();
                        mapper.Map(transaction, invoiceRequest);
                        invoiceRequest.InvoiceDetails = model.InvoiceDetails;

                        if (string.IsNullOrWhiteSpace(invoiceRequest.CardOwnerName) && consumer != null)
                        {
                            invoiceRequest.CardOwnerName = consumer.ConsumerName;
                        }

                        if (string.IsNullOrWhiteSpace(invoiceRequest.CardOwnerNationalID) && consumer != null)
                        {
                            invoiceRequest.CardOwnerNationalID = consumer.ConsumerNationalID;
                        }

                        invoiceRequest.MerchantID = terminal.MerchantID;

                        invoiceRequest.ApplyAuditInfo(httpContextAccessor);

                        invoiceRequest.Calculate();

                        await invoiceService.CreateEntity(invoiceRequest, dbTransaction: dbTransaction);

                        endResponse.InnerResponse = new OperationResponse(Transactions.Shared.Messages.InvoiceCreated, StatusEnum.Success, invoiceRequest.InvoiceID);

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
                        logger.LogError(ex, $"Failed to create invoice. TransactionID: {transaction.PaymentTransactionID}");

                        endResponse.InnerResponse = new OperationResponse($"{Transactions.Shared.Messages.FailedToCreateInvoice}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, "FailedToCreateInvoice", ex.Message);

                        await dbTransaction.RollbackAsync();
                    }
                }
            }

            try
            {
                await SendTransactionSuccessEmails(transaction, terminal);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"{nameof(ProcessTransaction)}: EmailSend");
            }

            return CreatedAtAction(nameof(GetTransaction), new { transactionID = transaction.PaymentTransactionID }, endResponse);
        }

        private async Task<ShvaTransactionDetails> GetLastShvaTransactionDetails(string shvaTerminalNumber)
        {
            return await this.transactionsService.GetTransactions().Where(x => x.ShvaTransactionDetails.ShvaTerminalID == shvaTerminalNumber)
                .OrderByDescending(d => d.TransactionDate).Select(d => d.ShvaTransactionDetails).FirstOrDefaultAsync();
        }

        private DocumentOriginEnum GetDocumentOrigin(Guid? billingDealID, Guid? paymentRequestID, bool pinpad)
        {
            if (pinpad)
            {
                return DocumentOriginEnum.Device;
            }
            else if (billingDealID.HasValue)
            {
                return DocumentOriginEnum.Billing;
            }
            else if (User.IsTerminal())
            {
                return DocumentOriginEnum.API;
            }
            else if (User.IsMerchant())
            {
                return DocumentOriginEnum.UI;
            }
            else if (paymentRequestID.HasValue)
            {
                return DocumentOriginEnum.PaymentRequest;
            }
            else if (User.IsAdmin())
            {
                // TODO: checkout identty
                return DocumentOriginEnum.Checkout;
            }
            else
            {
                // TODO: how to determine device
                return DocumentOriginEnum.Device;
            }
        }

        private async Task SendTransactionSuccessEmails(PaymentTransaction transaction, Merchants.Business.Entities.Terminal.Terminal terminal, string emailTo = null)
        {
            var settings = terminal.PaymentRequestSettings;

            var emailSubject = "Payment Success";
            var emailTemplateCode = nameof(PaymentTransaction);
            var substitutions = new List<TextSubstitution>
            {
                new TextSubstitution(nameof(settings.MerchantLogo), string.IsNullOrWhiteSpace(settings.MerchantLogo) ? $"{apiSettings.CheckoutPortalUrl}/img/merchant-logo.png" : settings.MerchantLogo),
                new TextSubstitution(nameof(terminal.Merchant.MarketingName), terminal.Merchant.MarketingName ?? terminal.Merchant.BusinessName),
                new TextSubstitution(nameof(transaction.TransactionDate), TimeZoneInfo.ConvertTimeFromUtc(transaction.TransactionDate.GetValueOrDefault(), UserCultureInfo.TimeZone).ToString("d")), // TODO: locale
                new TextSubstitution(nameof(transaction.TransactionAmount), $"{transaction.TotalAmount.ToString("F2")}{transaction.Currency.GetCurrencySymbol()}"),

                new TextSubstitution(nameof(transaction.ShvaTransactionDetails.ShvaTerminalID), transaction.ShvaTransactionDetails?.ShvaTerminalID ?? string.Empty),
                new TextSubstitution(nameof(transaction.ShvaTransactionDetails.ShvaShovarNumber), transaction.ShvaTransactionDetails?.ShvaShovarNumber ?? string.Empty),

                new TextSubstitution(nameof(transaction.CreditCardDetails.CardNumber), transaction.CreditCardDetails?.CardNumber ?? string.Empty),
                new TextSubstitution(nameof(transaction.CreditCardDetails.CardOwnerName), transaction.CreditCardDetails?.CardOwnerName ?? string.Empty),

                new TextSubstitution(nameof(transaction.DealDetails.DealDescription), transaction.DealDetails?.DealDescription ?? string.Empty)
            };

            var dictionaries = DictionariesService.GetDictionaries(CurrentCulture);

            var transactionTypeKey = typeof(TransactionTypeEnum).GetDataContractAttrForEnum(transaction.TransactionType.ToString());
            var cardPresenceKey = typeof(CardPresenceEnum).GetDataContractAttrForEnum(transaction.CardPresence.ToString());
            var originKey = typeof(DocumentOriginEnum).GetDataContractAttrForEnum(transaction.DocumentOrigin.ToString());

            var transactionTypeStr = dictionaries.TransactionTypeEnum[transactionTypeKey];
            var cardPresenceTypeStr = dictionaries.CardPresenceEnum[cardPresenceKey];
            var originStr = dictionaries.DocumentOriginEnum[originKey];

            substitutions.Add(new TextSubstitution(nameof(transaction.TransactionType), transactionTypeStr));
            substitutions.Add(new TextSubstitution(nameof(transaction.CardPresence), cardPresenceTypeStr));
            substitutions.Add(new TextSubstitution(nameof(transaction.DocumentOrigin), originStr));

            if (transaction.DealDetails?.ConsumerEmail == null)
            {
                throw new ArgumentNullException(nameof(transaction.DealDetails.ConsumerEmail));
            }

            var email = new Email
            {
                EmailTo = transaction.DealDetails.ConsumerEmail,
                Subject = emailSubject,
                TemplateCode = emailTemplateCode,
                Substitutions = substitutions.ToArray()
            };

            await emailSender.SendEmail(email);

            if (terminal.Settings.SendTransactionSlipEmailToMerchant == true && terminal.BillingSettings.BillingNotificationsEmails?.Count() > 0)
            {
                var merchantEmailTemplateCode = nameof(PaymentTransaction) + "Merchant";

                foreach (var notificationEmail in terminal.BillingSettings.BillingNotificationsEmails)
                {
                    var emailToMerchant = new Email
                    {
                        EmailTo = notificationEmail,
                        Subject = emailSubject,
                        TemplateCode = merchantEmailTemplateCode,
                        Substitutions = substitutions.ToArray()
                    };

                    await emailSender.SendEmail(emailToMerchant);
                }
            }
        }

        private async Task<OperationResponse> NextBillingDeal(BillingDeal billingDeal)
        {
            var token = EnsureExists(await keyValueStorage.Get(billingDeal.CreditCardToken.ToString()), "CreditCardToken");

            var transaction = mapper.Map<CreateTransactionRequest>(billingDeal);

            transaction.CardPresence = CardPresenceEnum.CardNotPresent;
            transaction.TransactionType = TransactionTypeEnum.RegularDeal;

            try
            {
                var actionResult = await ProcessTransaction(transaction, token, specialTransactionType: SpecialTransactionTypeEnum.RegularDeal, initialTransactionID: billingDeal.InitialTransactionID, billingDeal: billingDeal);

                var response = actionResult.Result as ObjectResult;
                return response.Value as OperationResponse;
            }
            catch (BusinessException businessEx)
            {
                logger.LogError($"{nameof(NextBillingDeal)}: {billingDeal.BillingDealID}, Error: {string.Join("; ", businessEx.Errors?.Select(b => $"{b.Code}:{b.Description}"))}");
                return new OperationResponse { Message = businessEx.Message, Status = StatusEnum.Error, Errors = businessEx.Errors };
            }
        }
    }
}