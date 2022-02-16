using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Security.KeyVault.Secrets;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using Transactions.Api.Extensions;
using Transactions.Api.Extensions.Filtering;
using Transactions.Api.Models.Billing;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Services;
using Transactions.Api.Validation;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Transactions.Shared;
using Transactions.Shared.Enums;
using Z.EntityFramework.Plus;
using SharedApi = Shared.Api;
using SharedBusiness = Shared.Business;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Controllers
{
    [Route("api/billing")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrManagerOrAdmin)]
    [ApiController]
    public class BillingController : ApiControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly ICreditCardTokenService creditCardTokenService;
        private readonly IMapper mapper;
        private readonly IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage;
        private readonly ApplicationSettings appSettings;
        private readonly ILogger logger;
        private readonly IProcessorResolver processorResolver;
        private readonly IBillingDealService billingDealService;
        private readonly IConsumersService consumersService;
        private readonly ITerminalsService terminalsService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly IQueue billingDealsQueue;
        private readonly ApiSettings apiSettings;
        private readonly IEmailSender emailSender;
        private readonly ICryptoServiceCompact cryptoServiceCompact;
        private readonly IPaymentIntentService paymentIntentService;
        private readonly BasicServices.Services.IExcelService excelService;

        public BillingController(
            ITransactionsService transactionsService,
            ICreditCardTokenService creditCardTokenService,
            IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage,
            IMapper mapper,
            ITerminalsService terminalsService,
            IOptions<ApplicationSettings> appSettings,
            ILogger<CardTokenController> logger,
            IProcessorResolver processorResolver,
            IBillingDealService billingDealService,
            IHttpContextAccessorWrapper httpContextAccessor,
            IConsumersService consumersService,
            IQueueResolver queueResolver,
            IOptions<ApiSettings> apiSettings,
            IEmailSender emailSender,
            ICryptoServiceCompact cryptoServiceCompact,
            IPaymentIntentService paymentIntentService,
            BasicServices.Services.IExcelService excelService)
        {
            this.transactionsService = transactionsService;
            this.creditCardTokenService = creditCardTokenService;
            this.keyValueStorage = keyValueStorage;
            this.mapper = mapper;

            this.terminalsService = terminalsService;
            this.appSettings = appSettings.Value;
            this.logger = logger;
            this.processorResolver = processorResolver;
            this.billingDealService = billingDealService;
            this.httpContextAccessor = httpContextAccessor;
            this.consumersService = consumersService;
            this.billingDealsQueue = queueResolver.GetQueue(QueueResolver.BillingDealsQueue);
            this.apiSettings = apiSettings.Value;
            this.emailSender = emailSender;

            this.cryptoServiceCompact = cryptoServiceCompact;
            this.paymentIntentService = paymentIntentService;
            this.excelService = excelService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = (httpContextAccessor.GetUser().IsAdmin() ? typeof(BillingDealSummaryAdmin) : typeof(BillingDealSummary))
                    .GetObjectMeta(BillingDealSummaryResource.ResourceManager, CurrentCulture)
            };
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<BillingDealSummary>>> GetBillingDeals([FromQuery] BillingDealsFilter filter)
        {
            var query = billingDealService.GetBillingDeals().Filter(filter);
            var numberOfRecordsFuture = query.DeferredCount().FutureValue();

            using (var dbTransaction = billingDealService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                if (httpContextAccessor.GetUser().IsAdmin())
                {
                    var response = new SummariesResponse<BillingDealSummaryAdmin>();

                    var summary = await mapper.ProjectTo<BillingDealSummaryAdmin>(query.OrderByDynamic(filter.SortBy ?? nameof(BillingDeal.BillingDealTimestamp), filter.SortDesc)
                        .ApplyPagination(filter)).ToListAsync();

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
                    response.NumberOfRecords = numberOfRecordsFuture.Value;
                    return Ok(response);
                }
                else
                {
                    var response = new SummariesResponse<BillingDealSummary>();

                    //TODO: ordering
                    response.Data = await mapper.ProjectTo<BillingDealSummary>(query.OrderByDynamic(filter.SortBy ?? nameof(BillingDeal.BillingDealTimestamp), filter.SortDesc).ApplyPagination(filter)).Future().ToListAsync();
                    response.NumberOfRecords = numberOfRecordsFuture.Value;

                    return Ok(response);
                }
            }
        }

        [HttpGet]
        [Route("$excel")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<OperationResponse>> GetBillingDealsExcel([FromQuery] BillingDealsFilter filter)
        {
            var query = billingDealService.GetBillingDeals().Filter(filter);

            using (var dbTransaction = billingDealService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                if (httpContextAccessor.GetUser().IsAdmin())
                {
                    var response = new SummariesResponse<BillingDealSummaryAdmin>();

                    var summary = await mapper.ProjectTo<BillingDealSummaryAdmin>(query.OrderByDynamic(filter.SortBy ?? nameof(BillingDeal.BillingDealTimestamp), filter.SortDesc)).ToListAsync();

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

                    var mapping = BillingDealSummaryResource.ResourceManager.GetExcelColumnNames<BillingDealSummaryAdmin>();
                    var res = await excelService.GenerateFile($"Admin/BillingDeals-{Guid.NewGuid()}.xlsx", "BillingDeals", summary, mapping);
                    return Ok(new OperationResponse { Status = SharedApi.Models.Enums.StatusEnum.Success, EntityReference = res });
                }
                else
                {
                    var data = await mapper.ProjectTo<BillingDealSummary>(query.OrderByDynamic(filter.SortBy ?? nameof(BillingDeal.BillingDealTimestamp), filter.SortDesc)).ToListAsync();
                    var mapping = BillingDealSummaryResource.ResourceManager.GetExcelColumnNames<BillingDealSummary>();
                    var res = await excelService.GenerateFile($"{User.GetMerchantID()}/BillingDeals-{Guid.NewGuid()}.xlsx", "BillingDeals", data, mapping);
                    return Ok(new OperationResponse { Status = SharedApi.Models.Enums.StatusEnum.Success, EntityReference = res });
                }
            }
        }

        [HttpGet]
        [Route("{BillingDealID}")]
        public async Task<ActionResult<BillingDealResponse>> GetBillingDeal([FromRoute] Guid billingDealID)
        {
            using (var dbTransaction = billingDealService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var dbBillingDeal = EnsureExists(await billingDealService.GetBillingDeals().FirstOrDefaultAsync(m => m.BillingDealID == billingDealID));

                var billingDeal = mapper.Map<BillingDealResponse>(dbBillingDeal);

                if (billingDeal.TerminalID.HasValue)
                {
                    billingDeal.TerminalName = await terminalsService.GetTerminals().Where(t => t.TerminalID == billingDeal.TerminalID.Value).Select(t => t.Label).FirstOrDefaultAsync();
                }

                if (dbBillingDeal.PaymentType == PaymentTypeEnum.Card && dbBillingDeal.CreditCardToken.HasValue)
                {
                    billingDeal.TokenNotAvailable = (await keyValueStorage.Get(dbBillingDeal.CreditCardToken.ToString())) == null;
                }

                return Ok(billingDeal);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> CreateBillingDeal([FromBody] BillingDealRequest model)
        {
            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(m => model.TerminalID == null || m.TerminalID == model.TerminalID));
            var consumer = EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == model.DealDetails.ConsumerID), "Consumer");

            BillingDealTerminalSettingsValidator.Validate(terminal.Settings, model);

            // Optional calculate if values were not specified
            model.Calculate(terminal.Settings.VATRate.GetValueOrDefault(0));

            var newBillingDeal = mapper.Map<BillingDeal>(model);
            newBillingDeal.Active = true;
            newBillingDeal.MerchantID = terminal.MerchantID;
            newBillingDeal.TerminalID = terminal.TerminalID;

            if (model.PaymentType == PaymentTypeEnum.Card)
            {
                if (!model.CreditCardToken.HasValue || model.CreditCardToken.Value == default)
                {
                    return BadRequest(new OperationResponse($"{model.CreditCardToken} required", StatusEnum.Error));
                }

                CreditCardTokenDetails token = null;
                if (terminal.Settings.SharedCreditCardTokens == true)
                {
                    token = EnsureExists(await creditCardTokenService.GetTokensShared(terminal.TerminalID).FirstOrDefaultAsync(d => d.CreditCardTokenID == model.CreditCardToken.Value && d.ConsumerID == consumer.ConsumerID), "CreditCardToken");
                }
                else
                {
                    token = EnsureExists(await creditCardTokenService.GetTokens().FirstOrDefaultAsync(d => d.CreditCardTokenID == model.CreditCardToken.Value && d.ConsumerID == consumer.ConsumerID && d.TerminalID == terminal.TerminalID), "CreditCardToken");
                }

                //Ensure that token is not removed from key vault
                EnsureExists(await keyValueStorage.Get(token.CreditCardTokenID.ToString()), "CreditCardToken");

                newBillingDeal.InitialTransactionID = token.InitialTransactionID;
                newBillingDeal.CreditCardDetails = new Business.Entities.CreditCardDetails();

                mapper.Map(token, newBillingDeal.CreditCardDetails);
            }
            else if (model.PaymentType == PaymentTypeEnum.Bank)
            {
                EnsureExists(model.BankDetails);
            }
            else
            {
                return BadRequest(new OperationResponse($"{model.PaymentType} payment type is not supported", StatusEnum.Error));
            }

            newBillingDeal.DealDetails.UpdateDealDetails(consumer, terminal.Settings, newBillingDeal, null);

            if (newBillingDeal.InvoiceDetails == null && newBillingDeal.IssueInvoice)
            {
                newBillingDeal.InvoiceDetails = new SharedIntegration.Models.Invoicing.InvoiceDetails { InvoiceType = terminal.InvoiceSettings.DefaultInvoiceType.GetValueOrDefault() };
            }

            // TODO: calculation

            newBillingDeal.ApplyAuditInfo(httpContextAccessor);

            newBillingDeal.NextScheduledTransaction = BillingDealTerminalSettingsValidator.ValidateSchedue(newBillingDeal.BillingSchedule, null);

            await billingDealService.CreateEntity(newBillingDeal);

            return CreatedAtAction(nameof(GetBillingDeal), new { BillingDealID = newBillingDeal.BillingDealID }, new OperationResponse(Messages.BillingDealCreated, StatusEnum.Success, newBillingDeal.BillingDealID));
        }

        [HttpPut]
        [Route("{BillingDealID}")]
        public async Task<ActionResult<OperationResponse>> UpdateBillingDeal([FromRoute] Guid billingDealID, [FromBody] BillingDealUpdateRequest model)
        {
            var billingDeal = EnsureExists(await billingDealService.GetBillingDealsForUpdate().FirstOrDefaultAsync(m => m.BillingDealID == billingDealID));

            if (billingDeal.InProgress != BillingProcessingStatusEnum.Pending)
            {
                return BadRequest(new OperationResponse(Messages.BillingInProgress, StatusEnum.Error, billingDealID));
            }

            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(m => model.TerminalID == null || m.TerminalID == model.TerminalID));
            var consumer = EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == model.DealDetails.ConsumerID), "Consumer");

            BillingDealTerminalSettingsValidator.Validate(terminal.Settings, model);

            if (model.IssueInvoice != true)
            {
                model.InvoiceDetails = null;
            }

            if (billingDeal.InvoiceOnly)
            {
                return BadRequest(new OperationResponse(Messages.ThisMethodCannotBeUsedForInvoiceOnlyBillings, StatusEnum.Error));
            }

            if (model.PaymentType != billingDeal.PaymentType)
            {
                return BadRequest(new OperationResponse(Messages.PaymentTypeCannotBeChanged, StatusEnum.Error));
            }

            EnsureExists(model.DealDetails);  // TODO: redo EnsureExists

            //if (model.DealDetails.ConsumerID != billingDeal.DealDetails.ConsumerID)
            //{
            //    return BadRequest(MessagesConsumerCannotBeChanged);
            //}

            if (model.PaymentType == PaymentTypeEnum.Card)
            {
                if (!model.CreditCardToken.HasValue)
                {
                    EnsureExists(model.CreditCardToken);
                }

                CreditCardTokenDetails token = null;
                if (terminal.Settings.SharedCreditCardTokens == true)
                {
                    token = EnsureExists(await creditCardTokenService.GetTokensShared(terminal.TerminalID).FirstOrDefaultAsync(d => d.CreditCardTokenID == model.CreditCardToken.Value && d.ConsumerID == consumer.ConsumerID), "CreditCardToken");
                }
                else
                {
                    token = EnsureExists(await creditCardTokenService.GetTokens().FirstOrDefaultAsync(d => d.CreditCardTokenID == model.CreditCardToken.Value && d.ConsumerID == consumer.ConsumerID && d.TerminalID == terminal.TerminalID), "CreditCardToken");
                }

                //Ensure that token is not removed from key vault
                EnsureExists(await keyValueStorage.Get(token.CreditCardTokenID.ToString()), "CreditCardToken");

                billingDeal.InitialTransactionID = token.InitialTransactionID;
                billingDeal.CreditCardDetails = new Business.Entities.CreditCardDetails();

                if (billingDeal.CreditCardToken != token.CreditCardTokenID)
                {
                    if (token.ReplacementOfTokenID == null && billingDeal.CreditCardToken != null)
                    {
                        token.ReplacementOfTokenID = billingDeal.CreditCardToken;
                        await creditCardTokenService.UpdateEntity(token);
                    }

                    await billingDealService.AddCardTokenChangedHistory(billingDeal, token.CreditCardTokenID);
                }

                mapper.Map(token, billingDeal.CreditCardDetails);
            }
            else if (model.PaymentType == PaymentTypeEnum.Bank)
            {
                EnsureExists(model.BankDetails); // TODO: redo EnsureExists
            }
            else
            {
                return BadRequest(new OperationResponse($"{model.PaymentType} payment type is not supported", StatusEnum.Error));
            }

            billingDeal.NextScheduledTransaction = BillingDealTerminalSettingsValidator.ValidateSchedue(model.BillingSchedule, billingDeal.HasError ? null : billingDeal.CurrentTransactionTimestamp);

            mapper.Map(model, billingDeal);

            billingDeal.DealDetails.UpdateDealDetails(consumer, terminal.Settings, billingDeal, null);

            if (billingDeal.InvoiceDetails == null && billingDeal.IssueInvoice)
            {
                billingDeal.InvoiceDetails = new SharedIntegration.Models.Invoicing.InvoiceDetails { InvoiceType = terminal.InvoiceSettings.DefaultInvoiceType.GetValueOrDefault() };
            }

            billingDeal.ApplyAuditInfo(httpContextAccessor);

            await billingDealService.UpdateEntity(billingDeal);

            return Ok(new OperationResponse(Messages.BillingDealUpdated, StatusEnum.Success, billingDealID));
        }

        /// <summary>
        /// Billing deal that only process invoice. Does not produce transaction
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/api/invoiceonlybilling")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> CreateBillingDealInvoice([FromBody] BillingDealInvoiceOnlyRequest model)
        {
            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(m => model.TerminalID == null || m.TerminalID == model.TerminalID));
            var consumer = EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == model.DealDetails.ConsumerID), "Consumer");

            BillingDealTerminalSettingsValidator.Validate(terminal.Settings, model);

            // Optional calculate if values were not specified
            model.Calculate(terminal.Settings.VATRate.GetValueOrDefault(0));

            var newBillingDeal = mapper.Map<BillingDeal>(model);
            newBillingDeal.Active = true;
            newBillingDeal.MerchantID = terminal.MerchantID;
            newBillingDeal.TerminalID = terminal.TerminalID;

            newBillingDeal.DealDetails.UpdateDealDetails(consumer, terminal.Settings, newBillingDeal, null);

            if (newBillingDeal.InvoiceDetails == null && newBillingDeal.IssueInvoice)
            {
                newBillingDeal.InvoiceDetails = new SharedIntegration.Models.Invoicing.InvoiceDetails { InvoiceType = terminal.InvoiceSettings.DefaultInvoiceType.GetValueOrDefault() };
            }

            // TODO: calculation

            newBillingDeal.ApplyAuditInfo(httpContextAccessor);

            newBillingDeal.NextScheduledTransaction = BillingDealTerminalSettingsValidator.ValidateSchedue(newBillingDeal.BillingSchedule, null);

            await billingDealService.CreateEntity(newBillingDeal);

            return CreatedAtAction(nameof(GetBillingDeal), new { BillingDealID = newBillingDeal.BillingDealID }, new OperationResponse(Messages.BillingDealCreated, StatusEnum.Success, newBillingDeal.BillingDealID));
        }

        [HttpPut]
        [Route("/api/invoiceonlybilling/{BillingDealID}")]
        public async Task<ActionResult<OperationResponse>> UpdateBillingDealInvoice([FromRoute] Guid billingDealID, [FromBody] BillingDealInvoiceOnlyUpdateRequest model)
        {
            var billingDeal = EnsureExists(await billingDealService.GetBillingDealsForUpdate().FirstOrDefaultAsync(m => m.BillingDealID == billingDealID));

            if (billingDeal.InProgress != BillingProcessingStatusEnum.Pending)
            {
                return BadRequest(new OperationResponse(Messages.BillingInProgress, StatusEnum.Error, billingDealID));
            }

            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(m => model.TerminalID == null || m.TerminalID == model.TerminalID));
            var consumer = EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == model.DealDetails.ConsumerID), "Consumer");

            BillingDealTerminalSettingsValidator.Validate(terminal.Settings, model);

            if (!billingDeal.InvoiceOnly)
            {
                return BadRequest(new OperationResponse(Messages.ThisMethodCannotBeUsedForRegularBillings, StatusEnum.Error));
            }

            if (model.PaymentType != billingDeal.PaymentType)
            {
                return BadRequest(new OperationResponse(Messages.PaymentTypeCannotBeChanged, StatusEnum.Error));
            }

            EnsureExists(model.DealDetails); // TODO: redo EnsureExists

            billingDeal.NextScheduledTransaction = BillingDealTerminalSettingsValidator.ValidateSchedue(model.BillingSchedule, billingDeal.HasError ? null : billingDeal.CurrentTransactionTimestamp);

            mapper.Map(model, billingDeal);

            billingDeal.DealDetails.UpdateDealDetails(consumer, terminal.Settings, billingDeal, null);

            if (billingDeal.InvoiceDetails == null && billingDeal.IssueInvoice)
            {
                billingDeal.InvoiceDetails = new SharedIntegration.Models.Invoicing.InvoiceDetails { InvoiceType = terminal.InvoiceSettings.DefaultInvoiceType.GetValueOrDefault() };
            }

            billingDeal.ApplyAuditInfo(httpContextAccessor);

            await billingDealService.UpdateEntity(billingDeal);

            return Ok(new OperationResponse(Messages.BillingDealUpdated, StatusEnum.Success, billingDealID));
        }

        [HttpPatch]
        [Route("{BillingDealID}/change-token/{tokenID}")]
        public async Task<ActionResult<OperationResponse>> UpdateBillingDealToken([FromRoute] Guid billingDealID, [FromRoute] Guid tokenID)
        {
            var billingDeal = EnsureExists(await billingDealService.GetBillingDealsForUpdate().FirstOrDefaultAsync(m => m.BillingDealID == billingDealID));

            if (billingDeal.InProgress != BillingProcessingStatusEnum.Pending)
            {
                return BadRequest(new OperationResponse(Messages.BillingInProgress, StatusEnum.Error, billingDealID));
            }

            var consumer = EnsureExists(
                await consumersService.GetConsumers()
                .FirstOrDefaultAsync(d => d.ConsumerID == billingDeal.DealDetails.ConsumerID), "Consumer");

            if (billingDeal.PaymentType != PaymentTypeEnum.Card)
            {
                return BadRequest(new OperationResponse($"{billingDeal.PaymentType} payment type is not supported", StatusEnum.Error));
            }

            var token = EnsureExists(
                await creditCardTokenService.GetTokens()
                .FirstOrDefaultAsync(d => d.TerminalID == billingDeal.TerminalID && d.CreditCardTokenID == tokenID && d.ConsumerID == consumer.ConsumerID), "CreditCardToken");

            billingDeal.InitialTransactionID = token.InitialTransactionID;
            billingDeal.CreditCardDetails = new Business.Entities.CreditCardDetails();
            billingDeal.Active = true;

            if (billingDeal.CreditCardToken != token.CreditCardTokenID)
            {
                if (token.ReplacementOfTokenID == null && billingDeal.CreditCardToken != null)
                {
                    token.ReplacementOfTokenID = billingDeal.CreditCardToken;
                    await creditCardTokenService.UpdateEntity(token);
                }

                await billingDealService.AddCardTokenChangedHistory(billingDeal, token.CreditCardTokenID);
            }

            mapper.Map(token, billingDeal.CreditCardDetails);

            billingDeal.ApplyAuditInfo(httpContextAccessor);

            await billingDealService.UpdateEntity(billingDeal);

            return Ok(new OperationResponse(Messages.BillingDealUpdated, StatusEnum.Success, billingDealID));
        }

        // It should be reworked to enable/disable methods pair
        [Obsolete]
        [HttpPost]
        [Route("{BillingDealID}/switch")]
        public async Task<ActionResult<OperationResponse>> SwitchBillingDeal([FromRoute] Guid billingDealID)
        {
            var billingDeal = EnsureExists(await billingDealService.GetBillingDealsForUpdate().FirstOrDefaultAsync(m => m.BillingDealID == billingDealID));

            if (billingDeal.InProgress != BillingProcessingStatusEnum.Pending)
            {
                return BadRequest(new OperationResponse(Messages.BillingInProgress, StatusEnum.Error, billingDealID));
            }

            if (billingDeal.NextScheduledTransaction == null)
            {
                return BadRequest(new OperationResponse(Messages.BillingDealIsClosed, StatusEnum.Error, billingDealID));
            }

            billingDeal.Active = !billingDeal.Active;

            billingDeal.ApplyAuditInfo(httpContextAccessor);

            await billingDealService.UpdateEntity(billingDeal);

            return Ok(new OperationResponse(Messages.BillingDealUpdated, StatusEnum.Success, billingDealID));
        }

        [HttpPost]
        [Route("disable-billing-deals")]
        public async Task<ActionResult<OperationResponse>> DisableBillingDeals([FromBody] DisableBillingDealsRequest request)
        {
            if (request.BillingDealsID == null || request.BillingDealsID.Count() == 0)
            {
                return BadRequest(new OperationResponse(Transactions.Shared.Messages.BillingDealsRequired, null, HttpContext.TraceIdentifier, nameof(request.BillingDealsID), Transactions.Shared.Messages.BillingDealsRequired));
            }

            if (request.BillingDealsID.Count() > appSettings.BillingDealsMaxBatchSize)
            {
                return BadRequest(new OperationResponse(string.Format(Messages.BillingDealsMaxBatchSize, appSettings.BillingDealsMaxBatchSize), null, httpContextAccessor.TraceIdentifier, nameof(request.BillingDealsID), string.Format(Messages.TransmissionLimit, appSettings.BillingDealsMaxBatchSize)));
            }

            int successfulCount = 0;

            foreach (var billingDealID in request.BillingDealsID)
            {
                var billingDeal = EnsureExists(await billingDealService.GetBillingDealsForUpdate().FirstOrDefaultAsync(m => m.BillingDealID == billingDealID));

                if (billingDeal.InProgress != BillingProcessingStatusEnum.Pending || billingDeal.NextScheduledTransaction == null)
                {
                    continue;
                }

                billingDeal.Active = !billingDeal.Active;

                billingDeal.ApplyAuditInfo(httpContextAccessor);

                await billingDealService.UpdateEntity(billingDeal);

                successfulCount++;
            }

            return Ok(new OperationResponse(string.Format(Messages.BillingDealsWereDisabled, successfulCount), StatusEnum.Success));
        }

        [HttpPost]
        [Route("activate-billing-deals")]
        public async Task<ActionResult<OperationResponse>> ActivateBillingDeals([FromBody] ActivateBillingDealsRequest request)
        {
            if (request.BillingDealsID == null || request.BillingDealsID.Count() == 0)
            {
                return BadRequest(new OperationResponse(Transactions.Shared.Messages.BillingDealsRequired, null, HttpContext.TraceIdentifier, nameof(request.BillingDealsID), Transactions.Shared.Messages.BillingDealsRequired));
            }

            if (request.BillingDealsID.Count() > appSettings.BillingDealsMaxBatchSize)
            {
                return BadRequest(new OperationResponse(string.Format(Messages.BillingDealsMaxBatchSize, appSettings.BillingDealsMaxBatchSize), null, httpContextAccessor.TraceIdentifier, nameof(request.BillingDealsID), string.Format(Messages.TransmissionLimit, appSettings.BillingDealsMaxBatchSize)));
            }

            int successfulCount = 0;

            foreach (var billingDealID in request.BillingDealsID)
            {
                var billingDeal = EnsureExists(await billingDealService.GetBillingDealsForUpdate().FirstOrDefaultAsync(m => m.BillingDealID == billingDealID));

                if (billingDeal.Active || billingDeal.InProgress != BillingProcessingStatusEnum.Pending || billingDeal.NextScheduledTransaction == null)
                {
                    continue;
                }

                billingDeal.Active = true;

                billingDeal.ApplyAuditInfo(httpContextAccessor);

                await billingDealService.UpdateEntity(billingDeal);

                successfulCount++;
            }

            return Ok(new OperationResponse(string.Format(Messages.BillingDealsWereActivated, successfulCount), StatusEnum.Success));
        }

        /// <summary>
        /// Used only by job
        /// </summary>
        /// <param name="terminalID"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("due-billings/{terminalID:guid}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Policy = Policy.AnyAdmin)]
        public async Task<ActionResult<SendBillingDealsToQueueResponse>> SendDueBillingDealsToQueue(Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));

            if (terminal.Status == Merchants.Shared.Enums.TerminalStatusEnum.Disabled || !terminal.EnabledFeatures.Contains(Merchants.Shared.Enums.FeatureEnum.Billing))
            {
                return new SendBillingDealsToQueueResponse
                {
                    Status = StatusEnum.Error,
                    Message = $"Terminal does not meet requirements. Status: {terminal.Status} is incorrect or Billing feature is not enabled"
                };
            }

            // send expiration emails
            await ProcessExpiredCardsBillingDeals(terminal);

            if (terminal.BillingSettings?.CreateRecurrentPaymentsAutomatically == true)
            {
                return await ProcessSendDueBillingDealsToQueue(terminalID);
            }
            else
            {
                return new SendBillingDealsToQueueResponse { Status = StatusEnum.Success, Count = 0 };
            }
        }

        [HttpPost]
        [Route("trigger-by-terminal/{terminalID:guid}")]
        public async Task<ActionResult<SendBillingDealsToQueueResponse>> TriggerBillingDealsByTerminal(Guid terminalID)
            => await ProcessSendDueBillingDealsToQueue(terminalID);

        /// <summary>
        /// Start process of creating transactions from billing deals
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("trigger-billing-deals")]
        public async Task<ActionResult<SendBillingDealsToQueueResponse>> CreateTransactionsFromBillingDeals(CreateTransactionFromBillingDealsRequest request)
        {
            if (request.BillingDealsID == null || request.BillingDealsID.Count() == 0)
            {
                return BadRequest(new OperationResponse(Transactions.Shared.Messages.BillingDealsRequired, null, HttpContext.TraceIdentifier, nameof(request.BillingDealsID), Transactions.Shared.Messages.BillingDealsRequired));
            }

            if (request.BillingDealsID.Count() > appSettings.BillingDealsMaxBatchSize)
            {
                return BadRequest(new OperationResponse(string.Format(Messages.BillingDealsMaxBatchSize, appSettings.BillingDealsMaxBatchSize), null, httpContextAccessor.TraceIdentifier, nameof(request.BillingDealsID), string.Format(Messages.TransmissionLimit, appSettings.BillingDealsMaxBatchSize)));
            }

            var filter = new BillingDealsFilter
            {
                Actual = true,
            };

            var billings = billingDealService.GetBillingDeals().Filter(filter).Where(b => request.BillingDealsID.Contains(b.BillingDealID));

            var allTerminals = await billings.Select(d => d.TerminalID).Distinct().ToListAsync();

            foreach (var terminalID in allTerminals)
            {
                var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));

                if (terminal.Status == Merchants.Shared.Enums.TerminalStatusEnum.Disabled || !terminal.EnabledFeatures.Contains(Merchants.Shared.Enums.FeatureEnum.Billing))
                {
                    return new SendBillingDealsToQueueResponse
                    {
                        Status = StatusEnum.Error,
                        Message = $"Terminal does not meet requirements. Status: {terminal.Status} is incorrect or Billing feature is not enabled"
                    };
                }
            }

            var allBillngs = await billings.Select(d => d.BillingDealID).ToListAsync();

            billings.Update(d => new BillingDeal { InProgress = BillingProcessingStatusEnum.Started });

            var numberOfRecords = allBillngs.Count();

            for (int i = 0; i < numberOfRecords; i += appSettings.BillingDealsMaxBatchSize)
            {
                await billingDealsQueue.PushToQueue(
                    allBillngs.Skip(i).Take(appSettings.BillingDealsMaxBatchSize));
            }

            return new SendBillingDealsToQueueResponse
            {
                Status = StatusEnum.Success,
                Message = Messages.TransactionsQueued.Replace("@count", numberOfRecords.ToString()),
                Count = numberOfRecords
            };
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("{billingDealID}/history")]
        [Authorize(Policy = Policy.AnyAdmin)]
        public async Task<ActionResult<SummariesResponse<BillingDealHistoryResponse>>> GetBillingDealHistory([FromRoute] Guid billingDealID)
        {
            EnsureExists(await billingDealService.GetBillingDeals()
                .Where(m => m.BillingDealID == billingDealID).Select(d => d.BillingDealID).FirstOrDefaultAsync());

            var query = billingDealService.GetBillingDealHistory(billingDealID);

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var numberOfRecords = query.DeferredCount().FutureValue();

                var response = new SummariesResponse<BillingDealHistoryResponse>();

                response.Data = await mapper.ProjectTo<BillingDealHistoryResponse>(query.OrderByDescending(t => t.OperationDate)).Future().ToListAsync();
                response.NumberOfRecords = numberOfRecords.Value;

                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{billingDealID}/pause")]
        public async Task<ActionResult<OperationResponse>> PauseBilling([FromRoute]Guid billingDealID, [FromBody] PauseBillingDealRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.DateTo < request.DateFrom)
            {
                ModelState.AddModelError(nameof(request.DateTo), $"Must be less than {nameof(request.DateFrom)}");
                return BadRequest(ModelState);
            }

            var billingDeal = await billingDealService.GetBillingDealsForUpdate().FirstOrDefaultAsync(b => b.BillingDealID == billingDealID);

            if (billingDeal.InProgress != BillingProcessingStatusEnum.Pending)
            {
                return BadRequest(new OperationResponse(Messages.BillingInProgress, StatusEnum.Error, billingDealID));
            }

            billingDeal.PausedFrom = request.DateFrom.Value;
            billingDeal.PausedTo = request.DateTo.Value;

            await billingDealService.UpdateEntityWithHistory(billingDeal, string.IsNullOrWhiteSpace(request.Reason) ? Messages.BillingDealPaused : request.Reason, BillingDealOperationCodesEnum.Paused);

            return Ok(new OperationResponse { Status = StatusEnum.Success, Message = Messages.BillingDealPaused, EntityUID = billingDealID });
        }

        [HttpPost]
        [Route("{billingDealID}/unpause")]
        public async Task<ActionResult<OperationResponse>> UnpauseBilling([FromRoute]Guid billingDealID)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var billingDeal = await billingDealService.GetBillingDealsForUpdate().FirstOrDefaultAsync(b => b.BillingDealID == billingDealID);

            if (billingDeal.NextScheduledTransaction == null)
            {
                return BadRequest(new OperationResponse(Messages.BillingDealIsClosed, StatusEnum.Error, billingDealID));
            }

            if (billingDeal.InProgress != BillingProcessingStatusEnum.Pending)
            {
                return BadRequest(new OperationResponse(Messages.BillingInProgress, StatusEnum.Error, billingDealID));
            }

            billingDeal.PausedFrom = billingDeal.PausedTo = null;

            await billingDealService.UpdateEntityWithHistory(billingDeal, Messages.BillingDealUnpaused, BillingDealOperationCodesEnum.Paused);

            return Ok(new OperationResponse { Status = StatusEnum.Success, Message = Messages.BillingDealUnpaused, EntityUID = billingDealID });
        }

        /// <summary>
        /// Sends billings to queue by terminal
        /// </summary>
        /// <param name="terminalID">Terminal ID</param>
        /// <returns></returns>
        private async Task<ActionResult<SendBillingDealsToQueueResponse>> ProcessSendDueBillingDealsToQueue(Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));

            if (terminal.Status == Merchants.Shared.Enums.TerminalStatusEnum.Disabled || !terminal.EnabledFeatures.Contains(Merchants.Shared.Enums.FeatureEnum.Billing))
            {
                return new SendBillingDealsToQueueResponse
                {
                    Status = StatusEnum.Error,
                    Message = $"Terminal does not meet requirements. Status: {terminal.Status} is incorrect or Billing feature is not enabled"
                };
            }

            var filter = new BillingDealsFilter
            {
                Actual = true,
                TerminalID = terminal.TerminalID
            };

            var billings = billingDealService.GetBillingDeals().Filter(filter);

            var allBillngs = await billings.Select(d => d.BillingDealID).ToListAsync();

            billings.Update(d => new BillingDeal { InProgress = BillingProcessingStatusEnum.Started });

            var numberOfRecords = allBillngs.Count();

            for (int i = 0; i < numberOfRecords; i += appSettings.BillingDealsMaxBatchSize)
            {
                await billingDealsQueue.PushToQueue(
                    allBillngs.Skip(i).Take(appSettings.BillingDealsMaxBatchSize));
            }

            return new SendBillingDealsToQueueResponse
            {
                Status = StatusEnum.Success,
                Message = Messages.TransactionsQueued.Replace("@count", numberOfRecords.ToString()),
                Count = numberOfRecords
            };
        }

        /// <summary>
        /// Retrieves billing deals for queue and sends credit card expired emails to customers if required.
        /// </summary>
        /// <returns></returns>
        private async Task<IEnumerable<BillingDealQueueEntry>> ProcessExpiredCardsBillingDeals(Terminal terminal)
        {
            var filter = new BillingDealsFilter
            {
                OnlyActive = true,
                TerminalID = terminal.TerminalID,
            };

            // TODO: config
            var dateToCheckExpiration = DateTime.Today.Date.AddMonths(1);

            // selected only active billings with expired tokens
            var allBillings = await billingDealService.GetBillingDeals()
                .Filter(filter)
                .Where(d => d.PaymentType == PaymentTypeEnum.Card && d.InvoiceOnly == false)
                .Join(creditCardTokenService.GetTokens(true).Where(d => d.ExpirationDate <= dateToCheckExpiration), o => o.CreditCardToken, i => i.CreditCardTokenID, (l, r) => new { BillingDeal = l, CardToken = r })
                .ToListAsync();

            var response = new List<BillingDealQueueEntry>();

            if (allBillings.Count == 0)
            {
                return response;
            }

            foreach (var dealEntity in allBillings)
            {
                if (dealEntity != null)
                {
                    logger.LogWarning($"Billing Deal {dealEntity.BillingDeal?.BillingDealID} credit card {CreditCardHelpers.GetCardBin(dealEntity.BillingDeal?.CreditCardDetails.CardNumber)} has expired ({dealEntity.BillingDeal?.CreditCardDetails.CardExpiration}). Setting it as inactive.");

                    try
                    {
                        // TODO: Add to history
                        await SendBillingDealCreditCardTokenExpiredEmail(dealEntity.BillingDeal, terminal);

                        response.Add(new BillingDealQueueEntry { TerminalID = terminal.TerminalID, BillingDealID = (dealEntity.BillingDeal?.BillingDealID).GetValueOrDefault() });
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Cannot send expiration email for billing {dealEntity.BillingDeal?.BillingDealID}: {ex.Message}");
                    }
                }
            }

            return response;
        }

        private string GetBillingDealLink(Guid billingDealID)
        {
            var uriBuilder = new UriBuilder(apiSettings.MerchantsManagementApiAddress);
            uriBuilder.Path = $"/billing-deals/view/{billingDealID}";

            var aTag = new TagBuilder("a");

            aTag.MergeAttribute("href", uriBuilder.ToString());
            aTag.InnerHtml.Append(billingDealID.ToString());

            var writer = new System.IO.StringWriter();
            aTag.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);

            return writer.ToString();
        }

        private string GetBillingDealRenewLink(Guid paymentRequestID)
        {
            var uriBuilder = new UriBuilder(apiSettings.CheckoutPortalUrl);
            uriBuilder.Path = "/i";
            var encrypted = cryptoServiceCompact.EncryptCompact(paymentRequestID.ToByteArray());

            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["r"] = encrypted;
            uriBuilder.Query = query.ToString();

            return uriBuilder.ToString();
        }

        private async Task SendBillingDealCreditCardTokenExpiredEmail(BillingDeal billingDeal, Terminal terminal)
        {
            var settings = terminal.PaymentRequestSettings;

            //TODO: resources?
            var emailSubject = $"{terminal.Merchant.MarketingName ?? terminal.Merchant.BusinessName}: - פג תוקף כרטיס אשראי המשמש לעסקה מחזורית";
            var emailTemplateCode = "BillingDealCardExpired";

            //Creating payment intent with 0 amount so user can renew billing deal with new credit card
            var paymentIntent = mapper.Map<BillingDeal, PaymentRequest>(billingDeal);
            await paymentIntentService.SavePaymentIntent(paymentIntent);

            var substitutions = new List<TextSubstitution>
            {
                new TextSubstitution(nameof(settings.MerchantLogo), string.IsNullOrWhiteSpace(settings.MerchantLogo) ? $"{apiSettings.CheckoutPortalUrl}/img/merchant-logo.png" : $"{apiSettings.BlobBaseAddress}/{settings.MerchantLogo}"),
                new TextSubstitution(nameof(terminal.Merchant.MarketingName), terminal.Merchant.MarketingName ?? terminal.Merchant.BusinessName),

                new TextSubstitution(nameof(billingDeal.DealDetails.DealDescription), billingDeal.DealDetails?.DealDescription ?? string.Empty),

                new TextSubstitution(nameof(billingDeal.CreditCardDetails.CardNumber), billingDeal.CreditCardDetails?.CardNumber ?? string.Empty),
                new TextSubstitution(nameof(billingDeal.CreditCardDetails.CardOwnerName), billingDeal.CreditCardDetails?.CardOwnerName ?? string.Empty),
                new TextSubstitution(nameof(billingDeal.DealDetails.ConsumerID), billingDeal.DealDetails?.ConsumerID?.ToString() ?? string.Empty),
                new TextSubstitution(nameof(billingDeal.BillingDealID), GetBillingDealLink(billingDeal.BillingDealID)),
                new TextSubstitution("RenewLink", GetBillingDealRenewLink(paymentIntent.PaymentRequestID)),
            };

            if (!string.IsNullOrWhiteSpace(billingDeal.DealDetails?.ConsumerEmail))
            {
                var email = new Email
                {
                    EmailTo = billingDeal.DealDetails.ConsumerEmail,
                    Subject = emailSubject,
                    TemplateCode = emailTemplateCode,
                    Substitutions = substitutions.ToArray()
                };

                await emailSender.SendEmail(email);
            }

            if (terminal.BillingSettings.BillingNotificationsEmails?.Count() > 0)
            {
                var merchantEmailTemplateCode = "BillingDealCardExpiredMerchant";

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
    }
}
