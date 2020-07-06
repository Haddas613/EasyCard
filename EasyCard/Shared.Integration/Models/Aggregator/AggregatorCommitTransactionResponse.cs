using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class AggregatorCommitTransactionResponse
    {
        public bool Success { get; set; }

        /// <summary>
        /// TODO: error codes
        /// </summary>
        public string ErrorMessage { get; set; }

        public IEnumerable<Error> Errors { get; set; }

        public int? OriginalHttpResponseCode { get; set; }

        public RejectionReasonEnum RejectReasonCode { get; set; }
    }
}
