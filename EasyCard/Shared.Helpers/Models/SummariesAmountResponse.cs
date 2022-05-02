using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Models
{
    public class SummariesAmountResponse<T> : SummariesResponse<T>
    {
        public decimal? TotalAmountILS { get; set; }

        public decimal? TotalAmountUSD { get; set; }

        public decimal? TotalAmountEUR { get; set; }
    }
}
