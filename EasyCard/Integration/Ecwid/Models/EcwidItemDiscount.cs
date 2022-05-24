using System;
using System.Collections.Generic;
using System.Text;

namespace Ecwid.Models
{
    public class EcwidItemDiscount
    {
        public decimal? Total { get; set; }

        public EcwidItemDiscountInfo DiscountInfo { get; set; }
    }
}
