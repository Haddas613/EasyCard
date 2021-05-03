using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Integrations.ClearingHouse
{
    public class GetCHMerchantsRequest
    {
        public string MerchantName { get; set; }

        public long? MerchantID { get; set; }
    }
}
