using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class ProcessorTransactionResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorTransactionResponse"/> class.
        /// Use in case of success
        /// </summary>
        public ProcessorTransactionResponse()
        {
            Success = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorTransactionResponse"/> class.
        /// Use this in case of error response
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="rejectionReasonEnum"></param>
        public ProcessorTransactionResponse(string errorMessage, RejectionReasonEnum errorCode)
        {
            Success = false;
            ErrorMessage = errorMessage;
            RejectReasonCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorTransactionResponse"/> class.
        /// Use this in case of error response
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="errorCode"></param>
        public ProcessorTransactionResponse(string errorMessage, string errorCode)
        {
            Success = false;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Will be used ShvaCreateTransactionDetails
        /// </summary>
        public object ProcessorTransactionDetails { get; set; }

        public string ErrorMessage { get; set; }

        public string ErrorCode { get; set; }

        public RejectionReasonEnum RejectReasonCode { get; set; }

        public bool Success { get; set; }
    }
}
