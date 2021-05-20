using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shva
{
    public class ShvaChangePasswordResponse : ProcessorCreateTransactionResponse
    {
        public ShvaChangePasswordResponse()
            : base()
        {
        }

        public ShvaChangePasswordResponse(string errorMessage, RejectionReasonEnum errorCode, string errorCodeStr)
            : base(errorMessage, errorCode, errorCodeStr)
        {
        }

        public ShvaChangePasswordResponse(string errorMessage, string errorCode)
            : base(errorMessage, errorCode)
        {
        }

       


    }
}
