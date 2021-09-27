using Shared.Business;
using Shared.Business.Security;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    // TODO: audit fields
    public class MasavFile : IEntityBase<long>, IAuditEntity
    {
        public long MasavFileID { get; set; }

        public virtual ICollection<MasavFileRow> Rows { get; set; }

        public DateTime? MasavFileDate { get; set; }

        public DateTime? MasavFileTimestamp { get; set; }

        public DateTime? PayedDate { get; set; }

        public Guid? TerminalID { get; set; }

        public Guid? MerchantID { get; set; }

        public decimal? TotalAmount { get; set; }

        public string StorageReference { get; set; }

        public int? InstituteNumber { get; set; }

        public int? SendingInstitute { get; set; }

        public string InstituteName { get; set; }

        public CurrencyEnum Currency { get; set; }

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        public string CorrelationId { get; set; }

        public string SourceIP { get; set; }

        public long GetID() => MasavFileID;
    }
}
