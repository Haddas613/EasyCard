using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.User;
using Merchants.Shared.Enums;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Terminal
{
    public class Terminal : IEntityBase
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

        public Merchant.Merchant Merchant { get; set; }

        public string Label { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        public TerminalStatusEnum Status { get; set; }

        public DateTime? ActivityStartDate { get; set; }

        public DateTime? Created { get; set; }

        public TerminalSettings Settings { get; set; }

        public TerminalBillingSettings BillingSettings { get; set; }

        public virtual IEnumerable<TerminalExternalSystem> Integrations { get; set; }

        public virtual IEnumerable<Feature> EnabledFeatures { get; set; }

        public long GetID()
        {
            return TerminalID;
        }
    }
}
