using System;
using System.Collections.Generic;
using System.Text;

namespace Nayax.Models
{
    public class PairRequest
    {
        public string posName { get; set; }
        public string terminalID { get; set; }

        public PairRequest(string posName, string terminalID)
        {
            this.terminalID = terminalID;
            this.posName = posName;
        }
    }
}
