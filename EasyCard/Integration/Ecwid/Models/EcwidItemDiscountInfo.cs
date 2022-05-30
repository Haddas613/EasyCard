using System;
using System.Collections.Generic;
using System.Text;

namespace Ecwid.Models
{
    public class EcwidItemDiscountInfo
    {
        public decimal? Value { get; set; }

        /// <summary>
        /// Discount type: ABS or PERCENT
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Discount base, one of ON_TOTAL, ON_MEMBERSHIP, ON_TOTAL_AND_MEMBERSHIP, CUSTOM
        /// </summary>
        public string Base { get; set; }

        /// <summary>
        /// Minimum order subtotal the discount applies to
        /// </summary>
        public decimal? OrderTotal { get; set; }

        /// <summary>
        /// Description of a discount (for discounts with base == CUSTOM)
        /// </summary>
        public string Description { get; set; }
    }
}
