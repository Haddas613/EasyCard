using Newtonsoft.Json;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared.Integration.Models.PaymentDetails
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CreditCardPaymentDetails : PaymentDetails
    {
        public override PaymentTypeEnum PaymentType { get; set; } = PaymentTypeEnum.Card;

        [Required]
        [JsonProperty(PropertyName = "cardExpiration")]
        [JsonConverter(typeof(CardExpirationConverter))]
        public CardExpiration CardExpiration { get; set; }

        [JsonProperty(PropertyName = "cardNumber")]
        public string СardNumber { get; set; }

        [JsonProperty(PropertyName = "shovarNumber")]
        public string ShovarNumber { get; set; }

        [JsonProperty(PropertyName = "cardVendor")]
        public string CardVendor { get; set; }

        [JsonProperty(PropertyName = "voucherNumber")]
        public string VoucherNumber { get; set; }
    }
}