using Newtonsoft.Json;
using Shared.Api.Models.Binding;
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

        [Required]
        [JsonProperty(PropertyName = "cardNumber")]
        public string СardNumber { get; set; }

        [JsonProperty(PropertyName = "shovarNumber")]
        public string ShovarNumber { get; set; }

        [JsonProperty(PropertyName = "cardVendor")]
        public string CardVendor { get; set; }

        [JsonProperty(PropertyName = "cardOwnerName")]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [StringLength(50, MinimumLength = 2)]
        public string CardOwnerName { get; set; }

        [JsonProperty(PropertyName = "cardOwnerNationalID")]
        [StringLength(20)]
        public string CardOwnerNationalID { get; set; }
    }
}