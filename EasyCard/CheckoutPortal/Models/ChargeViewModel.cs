using Microsoft.AspNetCore.Mvc.ModelBinding;
using Shared.Helpers;
using Shared.Helpers.Models.Attributes;
using Shared.Integration.Models;
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

        [BindNever]
        public Guid? ConsumerID { get; set; }

        public CurrencyEnum Currency { get; set; }

        [Required]
        [Range(0.01, 10000000)]
        public decimal? Amount { get; set; }

        /// <summary>
        /// Number Of Installments
        /// </summary>
        [Range(1, 36)]
        public int? NumberOfPayments { get; set; }

        /// <summary>
        /// Initial installment payment
        /// </summary>
        [Range(0.01, 999999)]
        public decimal? InitialPaymentAmount { get; set; }

        [BindNever]
        [DataType(DataType.Currency)]
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// Amount of one instalment payment
        /// </summary>
        [Range(0.01, 999999)]
        public decimal? InstallmentPaymentAmount { get; set; }

        [StringLength(500)]
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Key for merchant's system - to have ability to validate redirect
        /// </summary>
        [StringLength(100)]
        public string ApiKey { get; set; }

        /// <summary>
        /// Payment request ID
        /// </summary>
        [StringLength(100)]
        public string PaymentRequest { get; set; }

        [Required]
        [StringLength(19, MinimumLength = 9)]
        [CreditCardPlus]
        public string CardNumber { get; set; }

        [Required]
        //[Shared.Helpers.Models.CardExpirationValidator]
        [RegularExpression("^ *[0-9]{2} */ *[0-9]{2} *$", ErrorMessageResourceName = "CardExpirationValidator", ErrorMessageResourceType = typeof(CheckoutPortal.Messages))]
        public string CardExpiration { get; set; }

        //[Required]
        [StringLength(4, MinimumLength = 3)]
        [RegularExpression("^[0-9]{3,4}$")]
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
        public IEnumerable<SavedTokenInfo> SavedTokens { get; set; }

        public bool? IssueInvoice { get; set; }

        public bool? AllowPinPad { get; set; }

        public bool PinPad { get; set; }

        public bool PayWithBit { get; set; }

        [BindNever]
        public int? MinCreditInstallments { get; set; }

        [BindNever]
        public int? MaxCreditInstallments { get; set; }

        [BindNever]
        public int? MinInstallments { get; set; }

        [BindNever]
        public int? MaxInstallments { get; set; }

        public TransactionTypeEnum TransactionType { get; set; }

        [BindNever]
        public IEnumerable<TransactionTypeEnum> TransactionTypes { get; set; }

        public string StateData { get; set; }

        public IEnumerable<PinPadDevice> PinPadDevices { get; set; }

        public string PinPadDeviceID { get; set; }

        [StringLength(100)]
        public string PaymentIntent { get; set; }

        [BindNever]
        public bool IsRefund { get; set; }

        public string ConnectionID { get; set; }

        [BindNever]
        public bool UserAmount { get; set; }

        [BindNever]
        public bool OnlyAddCard { get; set; }

        [BindNever]
        public ICollection<Merchants.Shared.Enums.FeatureEnum> EnabledFeatures { get; set; }

        [BindNever]
        public bool? AllowBit { get; set; }

        [BindNever]
        public bool? EnableThreeDS { get; set; }

        /// <summary>
        /// If 3DSecure raises error - continue flow without 3ds
        /// </summary>
        [BindNever]
        public bool? ContinueInCaseOf3DSecureError { get; set; }

        public string ThreeDSServerTransID { get; set; }
    }
}
