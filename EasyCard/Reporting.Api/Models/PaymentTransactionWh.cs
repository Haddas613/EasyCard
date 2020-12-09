using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Shared.Enums;

namespace Reporting.Api.Models
{
    public class PaymentTransactionWh
    {
        /// <summary>
        /// Primary transaction report row reference
        /// </summary>
        public Guid PaymentTransactionWhID { get; set; }

        /// <summary>
        /// Legal transaction day
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Terminal
        /// </summary>
        public Guid TerminalID { get; set; }

        /// <summary>
        /// Merchant
        /// </summary>
        public Guid MerchantID { get; set; }

        /// <summary>
        /// Payment Type
        /// </summary>
        public PaymentTypeEnum PaymentTypeEnum { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// Transaction origin
        /// </summary>
        public DocumentOriginEnum DocumentOrigin { get; set; }

        /// <summary>
        /// Transactions amount (in shekels)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Number of transactions withing given measures
        /// </summary>
        public int TransactionsCount { get; set; }
    }
}
