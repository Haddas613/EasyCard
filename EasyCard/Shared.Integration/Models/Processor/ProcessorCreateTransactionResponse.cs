using Shared.Helpers;
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
        /// <param name="errorCode"></param>
        /// <param name="errorCodeStr"></param>
        /// <param name="processorResult"></param>
        public ProcessorCreateTransactionResponse(string errorMessage, RejectionReasonEnum errorCode, string errorCodeStr, int? processorResult = null)
        {
            Success = false;
            ErrorMessage = errorMessage;
            RejectReasonCode = errorCode;
            Errors = new List<Error> { new Error { Code = errorCodeStr, Description = errorMessage } };
            ResultCode = processorResult;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorCreateTransactionResponse"/> class.
        /// Use this in case of error response
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="errorCodeStr"></param>
        /// <param name="processorResult"></param>
        public ProcessorCreateTransactionResponse(string errorMessage, string errorCodeStr, int? processorResult = null)
        {
            Success = false;
            ErrorMessage = errorMessage;
            Errors = new List<Error> { new Error { Code = errorCodeStr, Description = errorMessage } };
            ResultCode = processorResult;
        }

        public bool Success { get; set; }

        /// <summary>
        /// General error message which can be displayed to merchant
        /// </summary>
        public string ErrorMessage { get; set; }

        public IEnumerable<Error> Errors { get; set; }

        public int? OriginalHttpResponseCode { get; set; }

        public RejectionReasonEnum RejectReasonCode { get; set; }

        public int? ResultCode { get; set; }

        /// <summary>
        /// Retailer number (code) to tell when calling to TelToGetAuthNum
        /// </summary>
        public string CompRetailerNum { get; set; }

        /// <summary>
        /// Phone number to call when authorization code is required
        /// </summary>
        public string TelToGetAuthNum { get; set; }
    }
}
