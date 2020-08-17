using Shared.Business;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Billing
{
    public class CurrencyRate : IEntityBase<long>
    {
        public long CurrencyRateID { get; set; }

        public DateTime? Date { get; set; }

        public CurrencyEnum Currency { get; set; }

        public decimal? Rate { get; set; }

        public long GetID()
        {
            return CurrencyRateID;
        }
    }
}
