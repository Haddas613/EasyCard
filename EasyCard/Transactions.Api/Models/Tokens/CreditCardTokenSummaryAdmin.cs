using Newtonsoft.Json;
using Shared.Api.UI;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Tokens
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CreditCardTokenSummaryAdmin : CreditCardTokenSummary
    {
        public string MerchantName { get; set; }

        public string TerminalName { get; set; }
    }
}
