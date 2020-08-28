﻿using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shva
{
    public class ShvaCreateTransactionResponse : ProcessorCreateTransactionResponse
    {
        public ShvaCreateTransactionResponse()
            : base()
        {
        }

        public ShvaCreateTransactionResponse(string errorMessage, RejectionReasonEnum errorCode, string errorCodeStr)
            : base(errorMessage, errorCode, errorCodeStr)
        {
        }

        public ShvaCreateTransactionResponse(string errorMessage, string errorCode)
            : base(errorMessage, errorCode)
        {
        }

        /// <summary>
        ///  Shva Shovar Number
        /// </summary>
        public string ShvaShovarNumber { get; set; }

        /// <summary>
        /// Shva Deal ID
        /// </summary>
        public string ShvaDealID { get; set; }

        /// <summary>
        /// Shva Tran recordID
        /// </summary>
        public string ShvaTranRecord { get; set; }

        // TODO: how to get this ?
        public string AuthNum { get; set; }

        // TODO: how to get this ?
        public string AuthSolekNum { get; set; }

        /// <summary>
        /// Deal Date
        /// </summary>
        public DateTime? ShvaTransactionDate { get; set; }

        public SolekEnum Solek { get; set; }

        public CardVendorEnum CreditCardVendor { get; set; }
    }
}
