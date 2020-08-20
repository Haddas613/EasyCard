using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Merchant
{
    public class MerchantsFilter : FilterBase
    {
        public string BusinessName { get; set; }
        public string MerchantID { get; set; }
        public string MarketingName { get; set; }
        public string BusinessID { get; set; }
        public string ContactPerson { get; set; }

    }
}
