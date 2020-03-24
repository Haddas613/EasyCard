using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    public class ProcessTransactionOptions
    {
        public CreditCardSecureDetails CreditCardSecureDetails { get; set; }

        public long TerminalID { get; set; }

        public long MerchantID { get; set; }
    }
}
