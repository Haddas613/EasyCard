using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

namespace CheckoutPortal.Models
{
    public class BitPaymentViewModel : ChargeViewModel
    {
        /// <summary>
        /// ECNG transaction id
        /// </summary>
        public Guid PaymentTransactionID { get; set; }

        /// <summary>
        /// Bit transaction id (BitPaymentInitiationId)
        /// </summary>
        public string PaymentInitiationId { get; set; }

        /// <summary>
        /// Bit transaction serial id (BitTransactionSerialId)
        /// </summary>
        public string TransactionSerialId { get; set; }

        [BindNever]
        public string ApplicationSchemeIos { get; set; }

        [BindNever]
        public string ApplicationSchemeAndroid { get; set; }

        [BindNever]
        public bool IsMobile { get; set; }
    }
}
