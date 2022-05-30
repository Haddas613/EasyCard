using Shared.Helpers;
using Shared.Helpers.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Billing
{
    public class ItemSummary
    {
        public Guid ItemID { get; set; }

        public string ItemName { get; set; }

        public decimal? Price { get; set; }

        public CurrencyEnum Currency { get; set; }

        public string ExternalReference { get; set; }

        [ExcelIgnore]
        public string BillingDesktopRefNumber { get; set; }

        /// <summary>
        /// External ID inside https://woocommerce.com system
        /// </summary>
        [ExcelIgnore]
        public string WoocommerceID { get; set; }

        /// <summary>
        /// External ID inside https://www.ecwid.com system
        /// </summary>
        [ExcelIgnore]
        public string EcwidID { get; set; }
    }
}
