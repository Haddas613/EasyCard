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

        public string AcsURL { get; set; }

        public string AcsChallengeMandated { get; set; }

        public ChallengeRequest ChallengeRequest { get; set; }

        public string Base64EncodedChallengeRequest { get; set; }
    }
}
