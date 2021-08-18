using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nayax
{
    public class NayaxPreCreateTransactionResponse : ProcessorPreCreateTransactionResponse
    {
        public NayaxPreCreateTransactionResponse()
            : base()
        {
        }

        public NayaxPreCreateTransactionResponse(string errorMessage, RejectionReasonEnum errorCode, string errorCodeStr)
            : base(errorMessage, errorCode, errorCodeStr)
        {
        }

        public NayaxPreCreateTransactionResponse(string errorMessage, string errorCode)
            : base(errorMessage, errorCode)
        {
        }

        public string UID { get; set; }
    }
}
