using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class PinPadTransactionsDetails
    {
        public string PinPadTransactionID { get; set; }
        public string PinPadUpdateReceiptNumber { get; set; }
        public string PinPadCorrelationID { get; set; }
    }
}
