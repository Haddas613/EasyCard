using System;
using System.Collections.Generic;
using System.Text;

namespace Nayax.Models
{

    public class Phase1ResponseBody
    {
        /// <summary>
        /// o OK
        /// </summary>
        public string statusCode { get; set; }
        public string statusMessage { get; set; }
        public double amount { get; set; }
        public int mutag { get; set; }
        public int manpik { get; set; }
        public int solek { get; set; }
        public string cardNumber { get; set; }
        public int tranType { get; set; }
        public int posEntryMode { get; set; }
        public int creditTerms { get; set; }
        public string cardName { get; set; }
        public string uid { get; set; }
        public string sysTraceNumber { get; set; }
    }

}
