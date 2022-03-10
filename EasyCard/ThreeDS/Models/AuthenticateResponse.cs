using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Models
{
    public class AuthenticateResponse
    {
        public ErrorVersion errorDetails { get; set; }

        public AuthenticationRequest RequestData { get; set; }

        public AuthenticateResponse ResponseData { get; set; }
    }
}
