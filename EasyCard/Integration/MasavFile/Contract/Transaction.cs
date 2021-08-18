using System;
using System.Collections.Generic;
using System.Text;

namespace PoalimOnlineBusiness.Contract
{
    public class Transaction
    {
        public string Currency { get; set; }

        public decimal Amount { get; set; }

        public long Reference { get; set; }

        public string BeneficiaryNname { get; set; }

        public string AccountNumber { get; set; }

        public string Bankcode { get; set; }

        public string BranchNumber { get; set; }
    }
}
