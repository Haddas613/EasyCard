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
        public string CreditCardTokenID { get; set; }

        public string TerminalID { get; set; }

        public string MerchantID { get; set; }

        public string CardNumber { get; set; }

        public CardExpiration CardExpiration { get; set; }

        public string CardVendor { get; set; }

        public DateTime? Created { get; set; }
    }
}
