using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Models
{
    public class SummariesAmountResponse<T> : SummariesResponse<T>
    {
        public decimal? TotalAmount { get; set; }
    }
}
