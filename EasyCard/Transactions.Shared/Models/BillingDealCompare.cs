using System;

namespace Transactions.Shared.Models
{
    public class BillingDealCompare
    {
        public Guid TerminalID { get; set; }
        public Guid MerchantID { get; set; }
        public decimal TransactionAmount { get; set; }
        public Guid? OperationDoneByID { get; set; }
    }
}