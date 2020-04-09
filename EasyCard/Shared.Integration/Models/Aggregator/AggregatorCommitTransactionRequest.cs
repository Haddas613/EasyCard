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
    }
}
