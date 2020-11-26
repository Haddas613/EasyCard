using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CheckoutPortal.Models
{
    public class ChargeViewModel
    {
        public ChargeViewModel()
        {
            CardExpiration = new CardExpiration { Year = DateTime.UtcNow.Year, Month = DateTime.UtcNow.Month };
        }

        // TODO: validation
        public string DealDescription { get; set; }

        [Required]
        [StringLength(19, MinimumLength = 9)]
        [RegularExpression("^[0-9]*$")]
        public string CardNumber { get; set; }

        [Required]
        [Shared.Helpers.Models.CardExpirationValidator]
        public CardExpiration CardExpiration { get; set; }

        [StringLength(50, MinimumLength = 2)]
        public string CardOwnerName { get; set; }

        [StringLength(20)]
        public string CardOwnerNationalID { get; set; }

        // TODO: validation
        public string Email { get; set; }

        // TODO: validation
        public string Phone { get; set; }

        [StringLength(4, MinimumLength = 3)]
        [RegularExpression("^[0-9]*$")]
        public string Cvv { get; set; }

        public CurrencyEnum Currency { get; set; }

        public decimal Amount { get; set; }

        /// <summary>
        /// after code 3 or 4 user can insert this value from credit company
        /// </summary>
        [StringLength(10)]
        public string AuthNum { get; set; }

        // TODO: va;idation
        public string ReturnUrl { get; set; }

        // TODO: va;idation
        /// <summary>
        /// Key for merchant's system - to have ability to validate redirect
        /// </summary>
        public string SecurityKey { get; set; }

        public string PaymentRequestReference { get; set; }
    }
}
