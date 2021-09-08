using System;
using System.Collections.Generic;
using System.Text;

namespace Reporting.Shared.Models.Admin
{
    public class MerchantsTotals
    {
        public int? RowN { get; set; }

        public string MerchantName { get; set; }

        public Guid MerchantID { get; set; }

        public decimal? TotalAmount { get; set; }
    }
}
