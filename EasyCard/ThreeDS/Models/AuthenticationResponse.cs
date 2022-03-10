using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Models
{
    public class AuthenticationResponse
    {
        public string messageType { get; set; }
        public string threeDSServerTransID { get; set; }
        public string acsTransID { get; set; }
        public string acsReferenceNumber { get; set; }
        public string acsOperatorID { get; set; }
        public string authenticationValue { get; set; }
        public string dsReferenceNumber { get; set; }
        public string dsTransID { get; set; }
        public string eci { get; set; }
        public string messageVersion { get; set; }
        public string transStatus { get; set; }
        public string broadInfo { get; set; }
    }
}
