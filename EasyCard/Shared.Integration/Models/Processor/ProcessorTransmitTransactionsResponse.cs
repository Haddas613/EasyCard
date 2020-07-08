using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class ProcessorTransmitTransactionsResponse
    {
        // TODO: abstract IDs
        public IEnumerable<string> FailedTransactions { get; set; }

        /// <summary>
        /// Shva Transmission ID
        /// </summary>
        public string TransmissionReference { get; set; }
    }
}
