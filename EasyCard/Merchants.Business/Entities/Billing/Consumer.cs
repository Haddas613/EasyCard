using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Billing
{
    public class Consumer
    {
        public Guid ConsumerID { get; set; }

        public Guid MerchantID { get; set; }

        public bool Active { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        public string ConsumerEmail { get; set; }
    }
}
