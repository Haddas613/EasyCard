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
        private readonly IBlobStorageService blobStorageService;

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

        [Route("api/adminwebhook/deleteConsumerRelatedData/{consumerID:guid}")]
        [HttpDelete]
        public async Task<ActionResult<OperationResponse>> DeleteConsumerRelatedData(Guid consumerID)
        {
            var result = new OperationResponse { Message = "OK", Status = SharedApi.Models.Enums.StatusEnum.Success };

            var consumer = EnsureExists(consumersService.GetConsumers().FirstOrDefaultAsync(c => c.ConsumerID == consumerID));

            var activeBillings = await billingDealService.GetBillingDeals().Where(b => b.DealDetails.ConsumerID == consumerID && b.Active).Select(b => b.BillingDealID).ToListAsync();

            foreach (var billingId in activeBillings)
            {
                //Since it's active only, switch is guaranteed to mark them as inactive
                var actionResult = await billingController.SwitchBillingDeal(billingId);
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

        [HttpPost("api/adminwebhook/generateMasavFile/{terminalID:guid}")]
        public async Task<ActionResult<OperationResponse>> GenerateMasavFile(Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));

            var bankDetails = terminal.BankDetails;

            var fileDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;

            long? masavFileID = await masavFileService.GenerateMasavFile(bankDetails.Bank, bankDetails.BankBranch, bankDetails.BankAccount, fileDate);

            if (masavFileID.HasValue)
            {
                var masavFile = EnsureExists(await masavFileService.GetMasavFile(masavFileID.Value));

                MasavData masavData = mapper.Map<MasavData>(masavFile);

                using (var file = new MemoryStream())
                {
                    var str = masavData.ExportFile(file);

                    await file.FlushAsync();

                    file.Seek(0, SeekOrigin.Begin);

                    var fileReference = await blobStorageService.Upload("TODO: file name", file);

                    // TODO update masav file
                }
            }

            var response = new OperationResponse();

            return Ok(response);
        }
    }
}
