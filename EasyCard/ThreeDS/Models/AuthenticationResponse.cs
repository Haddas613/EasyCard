using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Models
{
    public class AuthenticationResponse
    {
        public string MessageType { get; set; }
        public string ThreeDSServerTransID { get; set; }
        public string AcsTransID { get; set; }
        public string AcsReferenceNumber { get; set; }
        public string AcsOperatorID { get; set; }
        public string AcsURL { get; set; }
        public string AuthenticationType { get; set; }
        public string AcsChallengeMandated { get; set; }
        public string DsReferenceNumber { get; set; }
        public string DsTransID { get; set; }
        public string MessageVersion { get; set; }
        public string TransStatus { get; set; }
        public string TransStatusReason { get; set; }
        public string BroadInfo { get; set; }
    }
}
