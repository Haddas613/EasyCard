using System;
using System.Collections.Generic;
using System.Text;

namespace MerchantsApi.Business.Entities
{
    public class Merchant
    {
        public long MerchantID { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        public string BusinessName { get; set; }

        public string MarketingName { get; set; }

        public string BusinessID { get; set; }

        public string ContactPerson { get; set; }

        public ICollection<Terminal> Terminals { get; set; }

        public string Users { get; set; }
        public DateTime? Created { get; set; }
    }
}
