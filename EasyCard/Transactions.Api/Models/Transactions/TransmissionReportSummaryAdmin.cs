using Shared.Api.UI;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    public class TransmissionReportSummaryAdmin : TransmissionReportSummary
    {
        public string MerchantName { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid MerchantID { get; set; }

        public string TerminalName { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid TerminalID { get; set; }

        [MetadataOptions(Hidden = true)]
        public new CurrencyEnum Currency { get; set; }
    }
}
