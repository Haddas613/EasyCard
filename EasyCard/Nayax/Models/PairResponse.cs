using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nayax.Models
{
    public class PairResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PairResponse"/> class.
        /// Use in case of success
        /// </summary>
        public PairResponse()
        {
            Success = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PairResponse"/> class.
        /// Use this in case of error response
        /// </summary>
        /// <param name="errorMessage"></param>
        public PairResponse(string errorMessage, string errorCodeStr)
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


    }
}
