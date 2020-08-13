using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Integration.Models
{
    public class Item
    {
        public Guid ItemID { get; set; }

        public string ItemName { get; set; }

        public decimal? Price { get; set; }

        public CurrencyEnum Currency { get; set; }

        public decimal? Quantity { get; set; }

        /// <summary>
        /// Row amount
        /// </summary>
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? Amount { get; set; }

        /// <summary>
        /// Tax rate (VAT)
        /// </summary>
        [Range(0.01, 1)]
        [DataType(DataType.Currency)]
        public decimal? TaxRate { get; set; }

        /// <summary>
        /// Tax amount
        /// </summary>
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? TaxAmount { get; set; }

        // TODO: sku
    }
}
