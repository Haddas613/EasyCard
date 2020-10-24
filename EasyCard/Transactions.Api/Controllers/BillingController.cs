using System;
using System.Collections.Generic;
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
using Shared.Helpers.KeyValueStorage;
using Shared.Helpers.Security;
using Shared.Integration.Exceptions;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
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

namespace Transactions.Api.Controllers
{
    [Route("api/billing")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
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
            IConsumersService consumersService)
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
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(BillingDealSummary)
                    .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Select(d => d.GetColMeta(BillingDealSummaryResource.ResourceManager, System.Globalization.CultureInfo.InvariantCulture))
                    .ToDictionary(d => d.Key)
            };
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<BillingDealSummary>>> GetBillingDeals([FromQuery] BillingDealsFilter filter)
        {
            var query = billingDealService.GetBillingDeals().Filter(filter);

            using (var dbTransaction = billingDealService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<BillingDealSummary> { NumberOfRecords = await query.CountAsync() };

                //TODO: ordering
                response.Data = await mapper.ProjectTo<BillingDealSummary>(query.OrderByDescending(b => b.BillingDealTimestamp).ApplyPagination(filter)).ToListAsync();

                return Ok(response);
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

                return Ok(billingDeal);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> CreateBillingDeal([FromBody] BillingDealRequest model)
        {
            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));
            var consumer = EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.TerminalID == terminal.TerminalID && d.ConsumerID == model.DealDetails.ConsumerID), "Consumer");
            var token = EnsureExists(await creditCardTokenService.GetTokens().FirstOrDefaultAsync(d => d.TerminalID == terminal.TerminalID && d.CreditCardTokenID == model.CreditCardToken && d.ConsumerID == consumer.ConsumerID), "CreditCardToken");

            var newBillingDeal = mapper.Map<BillingDeal>(model);
            newBillingDeal.Active = true;

            newBillingDeal.InitialTransactionID = token.InitialTransactionID;

            mapper.Map(token, newBillingDeal.CreditCardDetails);

            newBillingDeal.MerchantID = terminal.MerchantID;

            newBillingDeal.ApplyAuditInfo(httpContextAccessor);

            await billingDealService.CreateEntity(newBillingDeal);

            return CreatedAtAction(nameof(GetBillingDeal), new { BillingDealID = newBillingDeal.BillingDealID }, new OperationResponse(Messages.BillingDealCreated, StatusEnum.Success, newBillingDeal.BillingDealID));
        }

        [HttpPut]
        [Route("{BillingDealID}")]
        public async Task<ActionResult<OperationResponse>> UpdateBillingDeal([FromRoute] Guid billingDealID, [FromBody] BillingDealUpdateRequest model)
        {
            var billingDeal = EnsureExists(await billingDealService.GetBillingDeals().FirstOrDefaultAsync(m => m.BillingDealID == billingDealID));

            mapper.Map(model, billingDeal);

            billingDeal.ApplyAuditInfo(httpContextAccessor);

            await billingDealService.UpdateEntity(billingDeal);

            return Ok(new OperationResponse(Messages.BillingDealUpdated, StatusEnum.Success, billingDealID));
        }

        [HttpDelete]
        [Route("{BillingDealID}")]
        public async Task<ActionResult<OperationResponse>> DeleteBillingDeal([FromRoute] Guid billingDealID)
        {
            var billingDeal = EnsureExists(await billingDealService.GetBillingDeals().FirstOrDefaultAsync(m => m.BillingDealID == billingDealID));

            billingDeal.Active = false;

            billingDeal.ApplyAuditInfo(httpContextAccessor);

            await billingDealService.UpdateEntity(billingDeal);

            return Ok(new OperationResponse(Messages.BillingDealDeleted, StatusEnum.Success, billingDealID));
        }
    }
}
