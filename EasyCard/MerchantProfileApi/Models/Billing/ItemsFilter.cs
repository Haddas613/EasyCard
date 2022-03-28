using Newtonsoft.Json;
using Shared.Api.Models;
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
    public class ItemsFilter : FilterBase
    {
        public string Search { get; set; }

        public CurrencyEnum? Currency { get; set; }

        public Guid? TerminalID { get; set; }

        [StringLength(50)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string ExternalReference { get; set; }

        [SwaggerExclude]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string BillingDesktopRefNumber { get; set; }

        public string Origin { get; set; }

        /// <summary>
        /// External ID inside https://woocommerce.com system
        /// </summary>
        [StringLength(50)]
        public string WoocommerceID { get; set; }

        /// <summary>
        /// External ID inside https://www.ecwid.com system
        /// </summary>
        [StringLength(50)]
        public string EcwidID { get; set; }
    }
}
