using Shared.Api.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reporting.Shared.Models.Admin
{
    public class ThreeDSChallengeSummary
    {
        [MetadataOptions(Hidden = true)]
        public Guid TerminalID { get; set; }

        public string TerminalName { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid MerchantID { get; set; }

        public string MerchantName { get; set; }

        public int NumberOfChallengeRequests { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }
    }
}
