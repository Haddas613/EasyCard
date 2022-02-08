using System;
using System.ComponentModel.DataAnnotations;

namespace Transactions.Api.Models.External.Bit
{
    public class CaptureBitTransactionRequest
    {
        [Required]
        public Guid PaymentTransactionID { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string PaymentInitiationId { get; set; }

        public Guid? PaymentRequestID { get; set; }

        public Guid? PaymentIntentID { get; set; }
    }
}
