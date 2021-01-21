using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Merchant
{
    public class MerchantsFilter : FilterBase
    {
        public Guid? MerchantID { get; set; }

        public string Search { get; set; }
    }
}
