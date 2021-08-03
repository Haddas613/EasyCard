using Newtonsoft.Json;
using Shared.Api.Models.Enums;
using Shared.Integration.Models;
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
        public int RegularTransactionsCount { get; set; }

        public int RefundTransactionsCount { get; set; }

        public decimal RegularTransactionsAmount { get; set; }

        public decimal RefundTransactionsAmount { get; set; }
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
                return Math.Round(RepeatingCustomers / TotalAmount, 4);
            }
        }

        public decimal NewCustomersRate
        {
            get
            {
                if (TotalAmount == 0 || NewCustomers == 0) return 0;
                return Math.Round(NewCustomers / TotalAmount, 4);
            }
        }
    }

    public class TransactionTimelines
    {
        public IEnumerable<TransactionTimeline> GivenPeriod { get; set; }

        public IEnumerable<TransactionTimeline> AltPeriod { get; set; }

        public decimal? GivenPeriodMeasure { get; set; }

        public decimal? AltPeriodMeasure { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public DateTime? AltDateFrom { get; set; }

        public DateTime? AltDateTo { get; set; }

        public ReportGranularityEnum? Granularity { get; set; }
    }

    public class TransactionTimeline
    {
        public int? Year { get; set; }

        public object DimensionValue { get; set; }

        public string Dimension
        {
            get
            {
                return DimensionValue is DateTime? ? ((DateTime?)DimensionValue)?.ToString("dd/MM") : $"{Year}-{DimensionValue?.ToString()}";
            }
        }

        public decimal? Measure { get; set; }
    }

    public class PaymentTypeTotals
    {
        public PaymentTypeEnum? PaymentTypeEnum { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
