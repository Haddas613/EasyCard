using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Checkout
{
    public class ConsumerInfo
    {
        public Guid ConsumerID { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        public string ConsumerEmail { get; set; }

        public string ConsumerName { get; set; }

        public string ConsumerPhone { get; set; }

        public string ConsumerNationalID { get; set; }
    }
}
