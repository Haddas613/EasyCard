using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class AggregatorCreateTransactionResponse
    {
        public bool Success { get; set; }

        /// <summary>
        /// General error mesage which can be displayed to merchant
        /// </summary>
        public string ErrorMessage { get; set; }

        public IEnumerable<Api.Models.Error> Errors { get; set; }

        public int? OriginalHttpResponseCode { get; set; }

        public RejectionReasonEnum RejectReasonCode { get; set; }
    }
}
