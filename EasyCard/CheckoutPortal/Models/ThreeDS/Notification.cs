using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckoutPortal.Models.ThreeDS
{
    public class Notification
    {
        public string Cres { get; set; }

        public string Error { get; set; }
    }

    public class NotificationResult
    {
        public bool Success { get; set; }

        public string ThreeDSServerTransID { get; set; }

        public string Error { get; set; }
    }
}
