using AutoMapper;
using BasicServices.BlobStorage;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PoalimOnlineBusiness;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.KeyValueStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Tokens;
using Transactions.Business.Services;
using SharedApi = Shared.Api;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    [ApiExplorerSettings(IgnoreApi = true)]
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
        private readonly IMasavFileService masavFileService;
        private readonly IBlobStorageService masavFileSorageService;
        private readonly Shared.ApplicationSettings appSettings;

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
            CardTokenController cardTokenController,
            IMasavFileService masavFileService,
            IOptions<Shared.ApplicationSettings> appSettings)
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
            this.masavFileService = masavFileService;

            this.appSettings = appSettings.Value;

            // TODO: remove, make singleton
            this.masavFileSorageService = new BlobStorageService(this.appSettings.PublicStorageConnectionString, this.appSettings.MasavFilesStorageTable, this.logger);
        }

        [Route("api/adminwebhook/deleteConsumerRelatedData/{consumerID:guid}")]
        [HttpDelete]
        public async Task<ActionResult<OperationResponse>> DeleteConsumerRelatedData(Guid consumerID)
        {
            var result = new OperationResponse { Message = "OK", Status = SharedApi.Models.Enums.StatusEnum.Success };

            // TODO: move this to event handler
            var inactivatedBillins = await billingController.ActivateOrDeactivateBillingDeals(false, billingDealService.GetBillingDeals().Where(b => b.DealDetails.ConsumerID == consumerID && b.Active));

            // ECNG-1483
            //var tokens = await creditCardTokenService.GetTokens().Where(t => t.ConsumerID == consumerID).Select(t => t.CreditCardTokenID).ToListAsync();

            //foreach (var token in tokens)
            //{
            //    var responseDataToken = (await cardTokenController.DeleteToken(token.ToString())).GetOperationResponse();

            //    if (responseDataToken.Status != SharedApi.Models.Enums.StatusEnum.Success)
            //    {
            //        result.Status = SharedApi.Models.Enums.StatusEnum.Warning;
            //    }
            //}

            return Ok(result);
        }
    }
}
