using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Models
{
    public class AuthenticationResponseEnvelop
    {
        public ErrorVersion ErrorDetails { get; set; }

        public AuthenticationResponse ResponseData { get; set; }
    }
}
