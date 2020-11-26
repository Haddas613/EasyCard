using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CheckoutPortal.Models
{
    public class CardRequest
    {
        // TODO: validation
        /// <summary>
        /// Deal description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Consumer name
        /// </summary>
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; }

        [StringLength(20)]
        public string NationalID { get; set; }

        // TODO: validation
        public string Email { get; set; }

        // TODO: validation
        public string Phone { get; set; }

        public Guid? ConsumerID { get; set; }

        public CurrencyEnum Currency { get; set; }

        public decimal Amount { get; set; }

        // TODO: validation
        public string RedirectUrl { get; set; }

        // TODO: validation
        /// <summary>
        /// Key for merchant's system - to have ability to validate redirect
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Payment request ID
        /// </summary>
        public string PaymentRequest { get; set; }
    }
}
