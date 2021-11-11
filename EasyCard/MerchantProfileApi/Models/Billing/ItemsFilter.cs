using Shared.Api.Models;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Billing
{
    public class ItemsFilter : FilterBase
    {
        public string Search { get; set; }

        public CurrencyEnum? Currency { get; set; }

        public Guid? TerminalID { get; set; }
    }
}
