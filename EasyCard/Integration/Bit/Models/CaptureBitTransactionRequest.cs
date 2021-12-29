using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.Models
{
    [Obsolete("Remove if not needed")]
    public class CaptureBitTransactionRequest
    {
        public string PaymentTransactionID { get; set; }

        public string BitPaymentInitiationId { get; set; }

        public string CorrelationId { get; set; }
    }
}
