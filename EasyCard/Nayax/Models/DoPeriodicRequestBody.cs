using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nayax.Models
{
    public class DoPeriodicRequestBody
    {
        public string clientID { get; set; }
        public string terminalID { get; set; }
        public string posID { get; set; }

        [JsonProperty("params")]
        public Object[] paramss { get; set; }

        public DoPeriodicRequestBody(string clientID, string terminalID, string posID)
        {
            this.clientID = clientID;
            this.terminalID = terminalID;
            this.posID = posID;
            this.paramss = new object[1];
            paramss[0] = "ashrait";
        }
    }
}
