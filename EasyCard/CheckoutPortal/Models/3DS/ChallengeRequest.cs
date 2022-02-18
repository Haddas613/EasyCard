using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckoutPortal.Models._3DS
{
    public class ChallengeRequest
    {
        public string messageType { get; set; }
        public string threeDSServerTransID { get; set; }
        public string acsTransID { get; set; }
        public string challengeWindowSize { get; set; }
        public string messageVersion { get; set; }
    }
}
