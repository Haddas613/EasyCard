using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nayax
{
    public class NayaxCreateTransactionResponse : ProcessorCreateTransactionResponse
    {
        public NayaxCreateTransactionResponse()
            : base()
        {
        }

        public NayaxCreateTransactionResponse(string errorMessage, RejectionReasonEnum errorCode, string errorCodeStr)
            : base(errorMessage, errorCode, errorCodeStr)
        {
        }

        public NayaxCreateTransactionResponse(string errorMessage, string errorCode)
            : base(errorMessage, errorCode)
        {
        }

        /// <summary>
        /// Number
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
        //public DateTime? ShvaTransactionDate { get; set; }

        public SolekEnum Solek { get; set; }

        public CardVendorEnum CreditCardVendor { get; set; }

        public string CardNumber { get; set; }

        public CardExpiration CardExpiration { get; set; }
        public string PinPadTransactionID { get; set; }

    }
}
