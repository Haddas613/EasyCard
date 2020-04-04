using Newtonsoft.Json;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    /// <summary>
    /// Do not store full card number. Please use 123456****1234 pattern CreditCardHelpers.GetCardDigits()
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CreditCardDetails : CreditCardDetailsBase
    {
        public string CardBin { get; set; }
    }
}
