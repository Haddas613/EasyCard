using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Shared.Enums;

namespace Reporting.Shared.Models.Tokens
{
    public class TokenTransactionsSubResult
    {
        public Guid? CreditCardTokenID { get; set; }

        public Guid? PaymentTransactionID { get; set; }

        public int ProductionTransactions { get; set; }

        public int FailedTransactions { get; set; }

        public decimal TotalSum { get; set; }

        public decimal TotalRefund { get; set; }
    }
}
