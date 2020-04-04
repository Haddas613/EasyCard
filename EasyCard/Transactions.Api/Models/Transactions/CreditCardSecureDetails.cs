using Newtonsoft.Json;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CreditCardSecureDetails : CreditCardDetailsBase
    {
        [StringLength(4, MinimumLength = 3)]
        public string Cvv { get; set; }
    }
}
