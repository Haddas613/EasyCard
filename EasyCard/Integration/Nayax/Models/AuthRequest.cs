using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Nayax.Models
{
    public class AuthRequest
    {
        [Required]
        public Guid? ECTerminalID { get; set; }
        public string terminalID { get; set; }

        public AuthRequest(string terminalID, Guid ECTerminalID)
        {
            this.terminalID = terminalID;
            this.ECTerminalID = ECTerminalID;
        }
    }
}
