using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class ProcessorCreateTransactionResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorCreateTransactionResponse"/> class.
        /// Use in case of success
        /// </summary>
        public ProcessorCreateTransactionResponse()
        {
            Success = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorCreateTransactionResponse"/> class.
        /// Use this in case of error response
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="rejectionReasonEnum"></param>
        public ProcessorCreateTransactionResponse(string errorMessage, RejectionReasonEnum errorCode, string errorCodeStr)
        {
            Success = false;
            ErrorMessage = errorMessage;
            RejectReasonCode = errorCode;
            ErrorCode = errorCodeStr;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorCreateTransactionResponse"/> class.
        /// Use this in case of error response
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="errorCode"></param>
        public ProcessorCreateTransactionResponse(string errorMessage, string errorCode)
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
