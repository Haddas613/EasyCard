using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Currency
{
    public class CurrencyRateUpdateRequest
    {
        public DateTime? Date { get; set; }

        public CurrencyEnum Currency { get; set; }

        public decimal? Rate { get; set; }
    }
}
