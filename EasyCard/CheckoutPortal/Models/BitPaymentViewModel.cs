using System;
using System.ComponentModel.DataAnnotations;

namespace CheckoutPortal.Models
{
    public class BitPaymentViewModel
    {
        /// <summary>
        /// ECNG transaction id
        /// </summary>
        public Guid PaymentTransactionID { get; set; }

        /// <summary>
        /// Bit transaction id (BitPaymentInitiationId)
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string PaymentInitiationId { get; set; }

        /// <summary>
        /// Bit transaction serial id (BitTransactionSerialId)
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string TransactionSerialId { get; set; }

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

        [StringLength(100)]
        public string PaymentIntent { get; set; }

        public string ApplicationSchemeIos { get; set; }

        public string ApplicationSchemeAndroid { get; set; }
    }
}
