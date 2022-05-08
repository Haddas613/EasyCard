using System;
using System.Collections.Generic;
using System.Text;

namespace CheckTerminalSettings
{
    public class ApplicationSettings
    {
        public string EncrKey { get; set; }

        public string EncrIv { get; set; }

        public string PaymentSystemsConfigsTsv { get; set; }

        public string BillingsTsv { get; set; }
    }
}
