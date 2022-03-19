using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Models
{
    public class AuthenticationRequestResponseData
    {
        public AuthenticationResponse AuthenticationResponse { get; set; }

        public string PurchaseDate { get; set; }

        public string ThreeDSServerTransID { get; set; }

        public string TransStatus { get; set; }

        public string AuthenticationValue { get; set; }

        public string Eci { get; set; }
    }
}
