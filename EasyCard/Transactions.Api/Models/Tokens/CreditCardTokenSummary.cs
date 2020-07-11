using Newtonsoft.Json;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Tokens
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CreditCardTokenSummary
    {
        public Guid? CreditCardTokenID { get; set; }

        public Guid? TerminalID { get; set; }

        public Guid? MerchantID { get; set; }

        public string CardNumber { get; set; }

        [JsonConverter(typeof(CardExpirationConverter))]
        public CardExpiration CardExpiration { get; set; }

        public string CardVendor { get; set; }

        public DateTime? Created { get; set; }

        public string CardOwnerName { get; set; }
    }
}
