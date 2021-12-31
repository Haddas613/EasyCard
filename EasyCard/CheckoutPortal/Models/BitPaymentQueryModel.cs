using System;

namespace CheckoutPortal.Models
{
    public class BitPaymentQueryModel
    {
        /// <summary>
        /// ECNG transaction id (PaymentTransactionID)
        /// </summary>
        public Guid? Tid { get; set; }

        /// <summary>
        /// Bit transaction id (BitPaymentInitiationId)
        /// </summary>
        public string Bid { get; set; }
    }
}
