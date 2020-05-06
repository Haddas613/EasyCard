using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class ProcessorTransmitTransactionsRequest
    {
        /// <summary>
        /// Shva terminal settings
        /// </summary>
        public object ProcessorSettings { get; set; }

        /// <summary>
        /// Processor transaction IDs
        /// </summary>
        public IEnumerable<string> TransactionIDs { get; set; }

        /// <summary>
        /// Request ID
        /// </summary>
        public string CorrelationId { get; set; }
    }
}
