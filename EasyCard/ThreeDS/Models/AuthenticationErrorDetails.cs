using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Models
{
    public class AuthenticationErrorDetails
    {
        public string ThreeDSServerTransID { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorComponent { get; set; }

        public string ErrorDescription { get; set; }

        public string ErrorDetail { get; set; }

        public string ErrorMessageType { get; set; }
    }
}
