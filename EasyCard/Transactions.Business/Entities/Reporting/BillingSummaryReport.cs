using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities.Reporting
{
    public class BillingSummaryReport
    {
        public Guid BillingDealID { get; set; }

        public string ConsumerExternalReference { get; set; }

        public DateTime? NextScheduledTransaction { get; set; }

        public decimal TransactionAmount { get; set; }
    }
}
