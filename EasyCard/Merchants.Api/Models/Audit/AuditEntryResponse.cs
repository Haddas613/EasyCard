using Shared.Api.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Audit
{
    public class AuditEntryResponse
    {
        [MetadataOptions(Hidden = true)]
        public Guid MerchantHistoryID { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid? MerchantID { get; set; }

        public string MerchantName { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid? TerminalID { get; set; }

        public string TerminalName { get; set; }

        public DateTime OperationDate { get; set; }

        public string OperationDoneBy { get; set; }

        [MetadataOptions(Hidden = true)]
        public string OperationDoneByID { get; set; }

        public string OperationCode { get; set; }

        public string OperationDescription { get; set; }

        [MetadataOptions(Hidden = true)]
        public string SourceIP { get; set; }

        [MetadataOptions(Hidden = true)]
        public string ReasonForChange { get; set; }
    }
}
