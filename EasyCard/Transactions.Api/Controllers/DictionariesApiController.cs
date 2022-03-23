using AutoMapper;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Helpers;
using Shared.Helpers.Events;
using Shared.Helpers.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Transactions.Api.Models.Dictionaries;
using Transactions.Api.Services;
using SharedHelpers = Shared.Helpers;

namespace Transactions.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/dictionaries")]
    [ApiController]
    public class DictionariesApiController : ApiControllerBase
    {
        private readonly IMapper mapper;

        public DictionariesApiController(IMapper mapper)
        {
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("transaction")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<TransactionsDictionaries>> GetTransactionDictionaries()
        {
            var dictionaries = DictionariesService.GetDictionaries(CurrentCulture);
            return Ok(dictionaries);
        }

        [HttpGet]
        [Route("banks")]
        [ResponseCache(VaryByHeader = "Accept-Language", Duration = 3600)]
        public async Task<ActionResult<IEnumerable<BankDictionaryDetails>>> GetBanks()
        {
            return Ok(BankHelper.GetListOfBankOptions(CurrentCulture));
        }

        [HttpGet]
        [Route("webhooks")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<IEnumerable<SharedHelpers.WebHooks.WebHookConfiguration>>> AvailableWebhooks()
        {
            var webhooks = new List<SharedHelpers.WebHooks.WebHookConfiguration>()
            {
                new SharedHelpers.WebHooks.WebHookConfiguration { EntityType = nameof(Business.Entities.PaymentTransaction), EventName = CustomEvent.TransactionCreated, IsFailureEvent = false },
                new SharedHelpers.WebHooks.WebHookConfiguration { EntityType = nameof(Business.Entities.PaymentTransaction), EventName = CustomEvent.TransactionRejected, IsFailureEvent = true },
                new SharedHelpers.WebHooks.WebHookConfiguration { EntityType = nameof(Business.Entities.BillingDeal), EventName = CustomEvent.BillingDealCreated, IsFailureEvent = false },
                new SharedHelpers.WebHooks.WebHookConfiguration { EntityType = nameof(Business.Entities.BillingDeal), EventName = CustomEvent.BillingDealUpdated, IsFailureEvent = false },
                new SharedHelpers.WebHooks.WebHookConfiguration { EntityType = nameof(Business.Entities.CreditCardTokenDetails), EventName = CustomEvent.CardTokenCreated, IsFailureEvent = false },
                new SharedHelpers.WebHooks.WebHookConfiguration { EntityType = nameof(Business.Entities.CreditCardTokenDetails), EventName = CustomEvent.CardTokenDeleted, IsFailureEvent = false },
                new SharedHelpers.WebHooks.WebHookConfiguration { EntityType = nameof(Merchants.Business.Entities.Billing.Consumer), EventName = CustomEvent.ConsumerCreated, IsFailureEvent = false },
                new SharedHelpers.WebHooks.WebHookConfiguration { EntityType = nameof(Merchants.Business.Entities.Billing.Consumer), EventName = CustomEvent.ConsumerUpdated, IsFailureEvent = false },
                new SharedHelpers.WebHooks.WebHookConfiguration { EntityType = nameof(Business.Entities.Invoice), EventName = CustomEvent.InvoiceGenerated, IsFailureEvent = false },
                new SharedHelpers.WebHooks.WebHookConfiguration { EntityType = nameof(Business.Entities.Invoice), EventName = CustomEvent.InvoiceGenerationFailed, IsFailureEvent = true },
            };

            return Ok(webhooks);
        }
    }
}
