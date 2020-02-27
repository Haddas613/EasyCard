using System;
using System.Collections.Generic;
using System.Text;

namespace Shva.Models
{
    // TODO: use real shva contract instead
    public class ShvaPaymentTransactionResponse
    {
        public string TransactionReference { get; set; }

        public string ErrorMessage { get; set; }

        public bool Success { get; set; }
    }
}
