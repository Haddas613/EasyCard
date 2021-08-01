using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Shared.Enums;

namespace Transactions.Business.Entities
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CreditCardPaymentDetails : PaymentDetails
    {
        public override PaymentTypeEnum PaymentType { get; set; } = PaymentTypeEnum.Card;

        [JsonProperty(PropertyName = "cardExpiration")]
        public string CardExpiration { get; set; }

        [JsonProperty(PropertyName = "cardNumber")]
        public string СardNumber { get; set; }
    }
}
