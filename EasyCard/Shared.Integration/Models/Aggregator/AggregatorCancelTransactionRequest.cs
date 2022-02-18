using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class AggregatorCancelTransactionRequest
    {
        public string RejectionReason { get; set; }

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

        public object AggregatorSettings { get; set; }

        /// <summary>
        /// Is Bit Transaction
        /// </summary>
        public bool IsBit { get; set; }
    }
}
