using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Shared.Enums;

namespace Reporting.Shared.Models
{
    public class MerchantDashboardResponse
    {
        public IEnumerable<TransactionsTotals> TransactionsTotals { get; set; }

        public IEnumerable<ItemsTotals> ItemsTotals { get; set; }

        public IEnumerable<ConsumersTotals> ConsumersTotals { get; set; }

        public IEnumerable<TransactionTimeline> TransactionTimeline { get; set; }

        public IEnumerable<PaymentTypeTotals> PaymentTypeTotal { get; set; }
    }

    public class TransactionsTotals
    {
        public int? TransactionsCount { get; set; }

        public decimal? TotalAmount { get; set; }
    }

    public class ItemsTotals
    {
        public int? RowN { get; set; }

        public string ItemName { get; set; }

        public decimal? TotalAmount { get; set; }
    }

    public class ConsumersTotals
    {
        public int CustomersCount { get; set; }

        public decimal AverageAmount { get; set; }

        public decimal RepeatingCustomers { get; set; }

        public decimal NewCustomers { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal RepeatingCustomersRate
        {
            get
            {
                if (TotalAmount == 0 || RepeatingCustomers == 0) return 0;
                return RepeatingCustomers / TotalAmount;
            }
        }

        public decimal NewCustomersRate
        {
            get
            {
                if (TotalAmount == 0 || NewCustomers == 0) return 0;
                return NewCustomers / TotalAmount;
            }
        }
    }

    public class TransactionTimeline
    {
        public DateTime? Date { get; set; }

        public decimal? TotalAmount { get; set; }
    }

    public class PaymentTypeTotals
    {
        public PaymentTypeEnum? PaymentTypeEnum { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
