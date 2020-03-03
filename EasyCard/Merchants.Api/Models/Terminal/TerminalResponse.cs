using Merchants.Api.Models.Merchant;
using Merchants.Api.Models.User;
using Merchants.Shared.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Terminal
{
    public class TerminalResponse
    {
        public TerminalResponse()
        {
            Settings = new TerminalSettings();
            BillingSettings = new TerminalBillingSettings();
        }

        public long TerminalID { get; set; }

        public long MerchantID { get; set; }

        public string Label { get; set; }

        public TerminalStatusEnum Status { get; set; }

        public DateTime? ActivityStartDate { get; set; }

        public DateTime? Created { get; set; }

        public MerchantResponse Merchant { get; set; }

        public IEnumerable<UserSummary> Users { get; set; }

        public IEnumerable<ExternalSystemDetails> Integrations { get; set; }

        public IEnumerable<FeatureResponse> EnabledFeatures { get; set; }

        // TODO: move settings to TerminalSettings class

        public JObject RedirectPageSettings { get; set; }

        public JObject PaymentButtonSettings { get; set; }

        public TerminalSettings Settings { get; set; }

        public TerminalBillingSettings BillingSettings { get; set; }
    }
}
