using Shared.Integration.Models;
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
    }
}
