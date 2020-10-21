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

        [Required]
        [StringLength(19, MinimumLength = 9)]
        [RegularExpression("^[0-9]*$")]
        public string CardNumber { get; set; }

        [Required]
        [Shared.Helpers.Models.CardExpirationValidator]
        public CardExpiration CardExpiration { get; set; }

        [StringLength(20)]
        public string CardVendor { get; set; }

        [StringLength(50, MinimumLength = 2)]
        public string CardOwnerName { get; set; }

        [StringLength(20)]
        public string CardOwnerNationalID { get; set; }

        // TODO: validation
        public string Email { get; set; }

        [StringLength(4, MinimumLength = 3)]
        [RegularExpression("^[0-9]*$")]
        public string Cvv { get; set; }

        /// <summary>
        /// after code 3 or 4 user can insert this value from credit company
        /// </summary>
        [StringLength(10)]
        public string AuthNum { get; set; }

        public string ReturnUrl { get; set; }
    }
}
