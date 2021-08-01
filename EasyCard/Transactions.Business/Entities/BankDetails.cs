using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    /// <summary>
    /// Represents abstract payment related to the bank, e.g. cheque or bank transfer
    /// </summary>
    public abstract class BankDetails : PaymentDetails
    {
        public int? Bank { get; set; }

        public int? BankBranch { get; set; }

        public int? BankAccount { get; set; }
    }
}
