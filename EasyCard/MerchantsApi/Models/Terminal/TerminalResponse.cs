using MerchantsApi.Models.User;
using Newtonsoft.Json.Linq;
using Shared.Models;
using Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantsApi.Models.Terminal
{
    public class TerminalResponse
    {
        public long TerminalID { get; set; }

        public string Label { get; set; }

        public TerminalStatusEnum Status { get; set; }

        public DateTime? ActivityStartDate { get; set; }

        public IEnumerable<UserSummary> Users { get; set; }

        public IEnumerable<ExternalSystemDetails> Integrations { get; set; }

        public IEnumerable<Feature> EnabledFeatures { get; set; }

        // TODO: move settings to TerminalSettings class

        public JObject RedirectPageSettings { get; set; }

        public JObject PaymentButtonSettings { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? MinInstallments { get; set; }

        /// <summary>
        /// If we set it to zero means installments blocked
        /// </summary>
        public int? MaxInstallments { get; set; }

        public int? MinCreditInstallments { get; set; }

        public int? MaxCreditInstallments { get; set; }

        public IEnumerable<string> BillingNotificationsEmails { get; set; } 

        public bool EnableDeletionOfUntransmittedTransactions { get; set; }

        public bool NationalIDRequired { get; set; }

        public bool CvvRequired { get; set; }
    }
}
