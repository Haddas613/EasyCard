using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class TransmissionInfo
    {
        public Guid PaymentTransactionID { get; set; }

        public string ShvaTranRecord { get; set; }
    }
}
