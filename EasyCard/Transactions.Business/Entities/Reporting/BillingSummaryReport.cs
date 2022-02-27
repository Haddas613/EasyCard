using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities.Reporting
{
    public class BillingSummaryReport
    {
        public string ExternalReference { get; set; }

        public int BillingDealsNumber { get; set; }

        public decimal TotalTransactionsAmounts { get; set; }
    }
}
