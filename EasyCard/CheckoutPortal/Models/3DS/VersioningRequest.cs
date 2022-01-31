using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckoutPortal.Models._3DS
{
    public class VersioningRequest
    {
        public string username { get; set; }
        public string Password { get; set; }
        public string PspID { get; set; }
        public string CardNumber { get; set; }
    }
}
