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
    }
}
