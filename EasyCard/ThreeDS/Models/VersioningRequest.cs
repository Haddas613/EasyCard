using System;
using System.Collections.Generic;
using System.Text;

namespace ThreeDS.Models
{
    public class VersioningRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PspID { get; set; }
        public string CardNumber { get; set; }
        public string NotificationURL { get; set; }
    }
}
