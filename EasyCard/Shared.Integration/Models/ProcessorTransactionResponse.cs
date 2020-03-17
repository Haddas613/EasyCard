using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class ProcessorTransactionResponse
    {
        public string TransactionReference { get; set; }

        public string DealNumber { get; set; }

        /// <summary>
        /// TODO: error codes
        /// </summary>
        public string ErrorMessage { get; set; }

        public int ProcessorCode { get; set; }

        public bool Success { get; set; }
    }
}
