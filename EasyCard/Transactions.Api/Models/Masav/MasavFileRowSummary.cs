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

        [MetadataOptions(Hidden = true)]
        public long? MasavFileID { get; set; }

        public Guid? PaymentTransactionID { get; set; }

        public long? Bankcode { get; set; }

        public long? BranchNumber { get; set; }

        public long? AccountNumber { get; set; }

        [MetadataOptions(Hidden = true)]
        public string NationalID { get; set; }

        public decimal? Amount { get; set; }

        [MetadataOptions(Hidden = true)]
        public bool? IsPayed { get; set; }

        public bool SmsSent { get; set; }

        [MetadataOptions(Hidden = true)]
        public DateTime? SmsSentDate { get; set; }

        [MetadataOptions(Hidden = true)]
        public CurrencyEnum Currency { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid? ConsumerID { get; set; }

        public string ConsumerName { get; set; }
    }
}
