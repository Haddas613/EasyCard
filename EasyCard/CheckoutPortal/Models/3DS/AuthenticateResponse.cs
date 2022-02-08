using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckoutPortal.Models._3DS
{
    public class AuthenticateResponse
    {
        public string threeDSServerTransID { get; set; }
        public string acsURL { get; set; }
        public string acsChallengeMandated { get; set; }
        public string transStatus { get; set; }
        public string authenticationValue { get; set; }
        public string eci { get; set; }
        public AuthenticationRequest authenticationRequest { get; set; }
        public AuthenticationResponse authenticationResponse { get; set; }
        public string purchaseDate { get; set; }

        public ChallengeRequest challengeRequest { get; set; }
        public string base64EncodedChallengeRequest { get; set; }
    }
}
