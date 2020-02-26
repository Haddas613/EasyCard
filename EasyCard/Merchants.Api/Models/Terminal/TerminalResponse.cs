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
            Settings = new TerminalResponseSettings();
            BillingSettings = new TerminalResponseBillingSettings();
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

        public IEnumerable<Feature> EnabledFeatures { get; set; }

        // TODO: move settings to TerminalSettings class

        public JObject RedirectPageSettings { get; set; }

        public JObject PaymentButtonSettings { get; set; }

        public TerminalResponseSettings Settings { get; set; }

        public TerminalResponseBillingSettings BillingSettings { get; set; }
    }

    public class TerminalResponseSettings
    {
        public int? MinInstallments { get; set; }

        /// <summary>
        /// If we set it to zero means installments blocked
        /// </summary>
        public int? MaxInstallments { get; set; }

        public int? MinCreditInstallments { get; set; }

        public int? MaxCreditInstallments { get; set; }

        public bool EnableDeletionOfUntransmittedTransactions { get; set; }

        public bool NationalIDRequired { get; set; }

        public bool CvvRequired { get; set; }
    }

    public class TerminalResponseBillingSettings
    {
        public IEnumerable<string> BillingNotificationsEmails { get; set; }
    }
}
