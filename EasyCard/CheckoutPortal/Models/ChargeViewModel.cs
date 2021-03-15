using Microsoft.AspNetCore.Mvc.ModelBinding;
using Shared.Helpers;
using Shared.Helpers.Models.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CheckoutPortal.Models
{
    public class ChargeViewModel
    {
        public string MarketingName { get; set; }

        /// <summary>
        /// Deal description
        /// </summary>
        [StringLength(250)]
        public string Description { get; set; }

        /// <summary>
        /// Consumer name
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; }

        [StringLength(20)]
        public string NationalID { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Phone]
        [StringLength(50)]
        public string Phone { get; set; }

        public Guid? ConsumerID { get; set; }

        public CurrencyEnum Currency { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal? Amount { get; set; }

        [StringLength(500)]
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Key for merchant's system - to have ability to validate redirect
        /// </summary>
        [Required]
        [StringLength(100)]
        public string ApiKey { get; set; }

        /// <summary>
        /// Payment request ID
        /// </summary>
        [StringLength(100)]
        public string PaymentRequest { get; set; }

        [Required]
        [StringLength(19, MinimumLength = 9)]
        [RegularExpression("^[0-9]*$")]
        [CreditCardPlus]
        public string CardNumber { get; set; }

        [Required]
        //[Shared.Helpers.Models.CardExpirationValidator]
        [RegularExpression("^ *[0-9]{2} */ *[0-9]{2} *$", ErrorMessageResourceName = "CardExpirationValidator", ErrorMessageResourceType = typeof(CheckoutPortal.Messages))]
        public string CardExpiration { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 3)]
        [RegularExpression("^[0-4]*$")]
        public string Cvv { get; set; }

        /// <summary>
        /// after code 3 or 4 user can insert this value from credit company
        /// </summary>
        [StringLength(10)]
        public string AuthNum { get; set; }

        /// <summary>
        /// Save credit card from request
        /// </summary>
        public bool SaveCreditCard { get; set; }

        /// <summary>
        /// Stored credit card details token (should be omitted in case if full credit card details used)
        /// </summary>
        public Guid? CreditCardToken { get; set; }

        [BindNever]
        public IEnumerable<KeyValuePair<Guid, string>> SavedTokens { get; set; }
    }
}
