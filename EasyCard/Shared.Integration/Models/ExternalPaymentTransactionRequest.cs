using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    // TODO: fill required fields
    public class ExternalPaymentTransactionRequest
    {
        public string TerminalReference { get; set; }

        public decimal Amount { get; set; }

        public string CreditCardNumber { get; set; }

        public string DealDescription { get; set; }
    }
}
