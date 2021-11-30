using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.MDBSource
{
    public class BillingSettings
    {
        public int billingDay { get;set;}
        public string MasavName { get; set; }
        public string MasavCode1 { get; set; }
        public string MasavCode2 { get; set; }
        public bool AddMaam { get; set; }
    }
}
