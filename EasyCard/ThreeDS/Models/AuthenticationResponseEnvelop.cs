﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Models
{
    public class AuthenticationResponseEnvelop
    {
        public AuthenticationErrorDetails ErrorDetails { get; set; }

        public AuthenticationRequestResponseData ResponseData { get; set; }



        public ChallengeRequest ChallengeRequest { get; set; }

        public string Base64EncodedChallengeRequest { get; set; }
    }
}
