using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantsApi.Models.Merchant
{
    public class MerchantBase
    {
        public long MerchantID { get; set; }

        public string BusinessName { get; set; }

        public string MerketingName { get; set; }

        public string BusinessID { get; set; }

        public string ContactPerson { get; set; }
    }
}
