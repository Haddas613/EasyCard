using Shared.Api.UI;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Masav
{
    public class MasavFileRowSummary
    {
        public long MasavFileRowID { get; set; }

        public long? MasavFileID { get; set; }

        public Guid? PaymentTransactionID { get; set; }

        public Guid? TerminalID { get; set; }

        public int? Bankcode { get; set; }

        public int? BranchNumber { get; set; }

        public int? AccountNumber { get; set; }

        [MetadataOptions(Hidden = true)]
        public string NationalID { get; set; }

        public decimal? Amount { get; set; }

        [MetadataOptions(Hidden = true)]
        public bool? IsPayed { get; set; }

        public bool SmsSent { get; set; }

        public decimal? ComissionTotal { get; set; }

        [MetadataOptions(Hidden = true)]
        public DateTime? SmsSentDate { get; set; }

        public DateTime? PayedDate { get; set; }

        [MetadataOptions(Hidden = true)]
        public CurrencyEnum Currency { get; set; }
    }
}
