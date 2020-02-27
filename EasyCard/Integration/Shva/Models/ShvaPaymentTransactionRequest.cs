using System;
using System.Collections.Generic;
using System.Text;

namespace Shva.Models
{
    // TODO: use real shva contract instead
    public class ShvaPaymentTransactionRequest
    {
        public string TerminalReference { get; set; }

        public decimal Amount { get; set; }

        public string CreditCardNumber { get; set; }

        public string DealDescription { get; set; }
    }
}
