using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class ProcessorTransmitTransactionsResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorTransmitTransactionsResponse"/> class.
        /// Use in case of success
        /// </summary>
        public ProcessorTransmitTransactionsResponse()
        {
            Success = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorTransmitTransactionsResponse"/> class.
        /// Use this in case of error response
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="rejectionReasonEnum"></param>
        public ProcessorTransmitTransactionsResponse(string errorMessage, RejectionReasonEnum errorCode, string errorCodeStr)
        {
            Success = false;
            ErrorMessage = errorMessage;
            RejectReasonCode = errorCode;
            Errors = new List<Error> { new Error { Code = errorCodeStr, Description = errorMessage } };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorTransmitTransactionsResponse"/> class.
        /// Use this in case of error response
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="errorCode"></param>
        public ProcessorTransmitTransactionsResponse(string errorMessage, string errorCodeStr)
        {
            Success = false;
            ErrorMessage = errorMessage;
            Errors = new List<Error> { new Error { Code = errorCodeStr, Description = errorMessage } };
        }

        public bool Success { get; set; }

        /// <summary>
        /// General error mesage which can be displayed to merchant
        /// </summary>
        public string ErrorMessage { get; set; }

        public IEnumerable<Error> Errors { get; set; }

        public int? OriginalHttpResponseCode { get; set; }

        public RejectionReasonEnum RejectReasonCode { get; set; }
    }
}
