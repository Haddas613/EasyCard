using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckoutPortal.Models
{
    public class CardRequest
    {
        public decimal Amount { get; set; }

        public CurrencyEnum Currency { get; set; }

        public string DealDescription { get; set; }

        public string ConsumerEmail { get; set; }

        public string ConsumerName { get; set; }

        public Guid? ConsumerID { get; set; }

        public string ConsumerPhone { get; set; }

        public string ConsumerNationalID { get; set; }

        /// <summary>
        /// Key for merchant's system - to have ability to validate redirect
        /// </summary>
        public string SecurityKey { get; set; }
    }
}
