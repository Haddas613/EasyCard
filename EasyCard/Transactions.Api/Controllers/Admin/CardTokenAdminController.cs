using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Security.KeyVault.Secrets;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using SharedHelpers = Shared.Helpers;

namespace Transactions.Api.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/cardtokensadmin")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    [ApiController]
    public class CardTokenAdminController : ApiControllerBase
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
        private readonly IBillingDealService billingDealService;

        public CardTokenAdminController(
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
            ISystemSettingsService systemSettingsService,
            IBillingDealService billingDealService)
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
            this.billingDealService = billingDealService;
        }

        /// <summary>
        /// Delete credit card token older than 5 years
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [Route("retentionCleanup")]
        public async Task<ActionResult<OperationResponse>> RetentionCleanup()
        {
            int numberOfDeletedTokens = 0;
            var oldDate = DateTime.Today.AddYears(-5);
            var query = await creditCardTokenService.GetTokens().Where(d => d.Active == true && d.Created < oldDate).ToListAsync();

            foreach (var token in query)
            {
                try
                {
                    await keyValueStorage.Delete(token.CreditCardTokenID.ToString());
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"{nameof(RetentionCleanup)}: Error while deleting token from keyvalue storage. Message: {e.Message}");
                }

                using (var dbTransaction = creditCardTokenService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
                {
                    try
                    {
                        token.Active = false;
                        await creditCardTokenService.UpdateEntity(token, dbTransaction);

                        // TODO: move to event handler
                        foreach (var billing in await billingDealService.GetBillingDeals().Filter(new Models.Billing.BillingDealsFilter { CreditCardTokenID = token.CreditCardTokenID }).ToListAsync())
                        {
                            billing.ResetToken();

                            await billingDealService.UpdateEntityWithHistory(billing, Messages.CreditCardTokenRemoved, BillingDealOperationCodesEnum.CreditCardTokenRemoved, dbTransaction);
                        }

                        dbTransaction.Commit();

                        numberOfDeletedTokens++;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"{nameof(RetentionCleanup)}: Error while deleting token and updating related data: {ex.Message}");
                        dbTransaction.Rollback();
                    }
                }
            }

            return Ok(new OperationResponse($"{numberOfDeletedTokens} tokens are deleted", StatusEnum.Success));
        }
    }
}