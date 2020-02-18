using System;
using System.Collections.Generic;
using System.Text;

namespace MerchantsApi.Business.Entities
{
    public class Merchant
    {
        public long MerchantID { get; set; }

        /// <summary>
        /// Optimistic concurrency key
        /// </summary>
        public byte[] UpdateTimestamp { get; set; }

        public ICollection<Terminal> Terminals { get; set; }
    }
}
