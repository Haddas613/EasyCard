using Shared.Integration.Models.Aggregator;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class AggregatorCommitTransactionRequest
    {
        /// <summary>
        /// Unique transaction ID
        /// </summary>
        public string TransactionID { get; set; }

        /// <summary>
        /// Request ID
        /// </summary>
        public string CorrelationId { get; set; }

        public string AggregatorTransactionID { get; set; }

        public string ConcurrencyToken { get; set; }

        public CreditCardDetails CreditCardDetails { get; set; }

        /// <summary>
        /// Shva details
        /// </summary>
        public object ProcessorTransactionDetails { get; set; }

        public object AggregatorSettings { get; set; }

        //public TransactionDetails TransactionDetails { get; set; }

        /// <summary>
        /// This transaction amount
        /// </summary>
        public decimal TransactionAmount { get; set; }

        /// <summary>
        /// Number Of Installments
        /// </summary>
        public int NumberOfInstallments { get; set; }

        /// <summary>
        /// Is Bit Transaction
        /// </summary>
        public bool IsBit { get; set; }
    }
}
