using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Nayax.Models
{
    public class AuthenticateRequest
    {
        [Required]
        public Guid? ECTerminalID { get; set; }
        public string OTP { get; set; }
        public string terminalID { get; set; }

        public AuthenticateRequest(string otp, string terminalID)
        {
            this.terminalID = terminalID;
            this.OTP = otp;
        }
    }
}
