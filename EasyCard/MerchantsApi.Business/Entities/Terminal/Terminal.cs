using System;
using System.Collections.Generic;
using System.Text;

namespace MerchantsApi.Business.Entities
{
    public class Terminal
    {
        public Terminal()
        {
            Settings = new TerminalSettings();
            BillingSettings = new TerminalBillingSettings();
            Integrations = new HashSet<TerminalExternalSystem>();
            EnabledFeatures = new HashSet<Feature>();
        }

        public long TerminalID { get; set; }

        public long MerchantID { get; set; }

        public Merchant Merchant { get; set; }

        public string Label { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        public TerminalStatusEnum Status { get; set; }

        public DateTime? ActivityStartDate { get; set; }
        public DateTime? Created { get; set; }

        public TerminalSettings Settings { get; set; }

        public TerminalBillingSettings BillingSettings { get; set; }

        public string Users { get; set; }

        public IEnumerable<TerminalExternalSystem> Integrations { get; set; }

        public IEnumerable<Feature> EnabledFeatures { get; set; }
    }
}
