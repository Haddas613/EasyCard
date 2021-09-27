using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    public class TransmitReportFilter : FilterBase
    {
        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public QuickDateFilterTypeEnum? QuickDateFilter { get; set; }

        public bool Success { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid? TerminalID { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid? MerchantID { get; set; }
    }
}
