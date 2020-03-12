using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class DealDetails
    {
        public string DealDescription { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        public string ConsumerEmail { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        public string ConsumerPhone { get; set; }

        /// <summary>
        /// We can know it from checkout page activity
        /// </summary>
        public string ConsumerIP { get; set; }

        public string MerchantIP { get; set; }
    }
}
