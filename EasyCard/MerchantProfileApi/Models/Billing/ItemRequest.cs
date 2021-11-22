using Newtonsoft.Json;
using Shared.Api.Models.Binding;
using Shared.Api.Swagger;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Billing
{
    public class ItemRequest
    {
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [Required(AllowEmptyStrings = false)]
        [StringLength(128, MinimumLength = 3)]
        public string ItemName { get; set; }

        public decimal Price { get; set; }

        public bool? Active { get; set; }

        public CurrencyEnum Currency { get; set; }

        [StringLength(50)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string ExternalReference { get; set; }

        [SwaggerExclude]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string BillingDesktopRefNumber { get; set; }

        [StringLength(50)]
        public string SKU { get; set; }
    }
}
