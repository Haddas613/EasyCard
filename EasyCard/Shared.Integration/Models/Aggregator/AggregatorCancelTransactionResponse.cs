﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class AggregatorCancelTransactionResponse
    {
        public bool Success { get; set; }

        /// <summary>
        /// TODO: error codes
        /// </summary>
        public string ErrorMessage { get; set; }

        public IEnumerable<Api.Models.Error> Errors { get; set; }

        public int? OriginalHttpResponseCode { get; set; }
    }
}
