using Shared.Business;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class MasavFileRow : IEntityBase<long>
    {
        public long MasavFileRowID { get; set; }

        public long? MasavFileID { get; set; }

        public virtual MasavFile MasavFile { get; set; }

        public Guid? PaymentTransactionID { get; set; }

        public Guid? TerminalID { get; set; }

        public int? Bankcode { get; set; }

        public int? BranchNumber { get; set; }

        public int? AccountNumber { get; set; }

        public string NationalID { get; set; }

        public decimal? Amount { get; set; }

        public bool? IsPayed { get; set; }

        public bool SmsSent { get; set; }

        public decimal? ComissionTotal { get; set; }

        public DateTime? SmsSentDate { get; set; }

        public DateTime? PayedDate { get; set; }

        public long GetID() => MasavFileRowID;
    }
}
