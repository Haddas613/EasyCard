using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class ExternalPaymentTransactionResponse
    {
        public string TransactionReference { get; set; }

        public string ErrorMessage { get; set; }

        public bool Success { get; set; }
    }
}
