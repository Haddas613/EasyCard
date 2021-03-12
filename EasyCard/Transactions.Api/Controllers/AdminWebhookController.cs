using AutoMapper;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api;
using Shared.Api.Models;
using Shared.Business.Security;
using Shared.Helpers.KeyValueStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Tokens;
using Transactions.Business.Services;
using SharedApi = Shared.Api;

namespace Transactions.Api.Controllers
{
    [Route("api/adminwebhook")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    [ApiController]
    public class AdminWebhookController : ApiControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly ICreditCardTokenService creditCardTokenService;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly IBillingDealService billingDealService;
        private readonly IConsumersService consumersService;
        private readonly ITerminalsService terminalsService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly BillingController billingController;
        private readonly CardTokenController cardTokenController;

        public AdminWebhookController(
            ITransactionsService transactionsService,
            ICreditCardTokenService creditCardTokenService,
            IMapper mapper,
            ITerminalsService terminalsService,
            ILogger<CardTokenController> logger,
            IBillingDealService billingDealService,
            IHttpContextAccessorWrapper httpContextAccessor,
            IConsumersService consumersService,
            BillingController billingController,
            CardTokenController cardTokenController)
        {
            this.transactionsService = transactionsService;
            this.creditCardTokenService = creditCardTokenService;
            this.mapper = mapper;

            this.logger = logger;
            this.terminalsService = terminalsService;
            this.billingDealService = billingDealService;
            this.httpContextAccessor = httpContextAccessor;
            this.consumersService = consumersService;

            this.billingController = billingController;
            this.cardTokenController = cardTokenController;
        }

        [Route("deleteConsumerRelatedData/{consumerID:guid}")]
        [HttpDelete]
        public async Task<ActionResult<OperationResponse>> DeleteConsumerRelatedData(Guid consumerID)
        {
            var result = new OperationResponse { Message = "OK", Status = SharedApi.Models.Enums.StatusEnum.Success };

            var consumer = EnsureExists(consumersService.GetConsumers().FirstOrDefaultAsync(c => c.ConsumerID == consumerID));

            var activeBillings = await billingDealService.GetBillingDeals().Where(b => b.DealDetails.ConsumerID == consumerID && b.Active).Select(b => b.BillingDealID).ToListAsync();

            foreach (var billingId in activeBillings)
            {
                var actionResult = await billingController.DeleteBillingDeal(billingId);
                var response = actionResult.Result as ObjectResult;
                var responseData = response.Value as OperationResponse;

                if (responseData.Status != SharedApi.Models.Enums.StatusEnum.Success)
                {
                    result.Status = SharedApi.Models.Enums.StatusEnum.Warning;
                }
            }

            var tokens = await creditCardTokenService.GetTokens().Where(t => t.ConsumerID == consumerID).Select(t => t.CreditCardTokenID).ToListAsync();

            foreach (var token in tokens)
            {
                var actionResult = await cardTokenController.DeleteToken(token.ToString());
                var response = actionResult.Result as ObjectResult;
                var responseData = response.Value as OperationResponse;

                if (responseData.Status != SharedApi.Models.Enums.StatusEnum.Success)
                {
                    result.Status = SharedApi.Models.Enums.StatusEnum.Warning;
                }
            }

            return Ok(result);
        }
    }
}
