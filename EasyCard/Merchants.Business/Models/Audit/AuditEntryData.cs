using Shared.Business.Audit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Models.Audit
{
    public class AuditEntryData
    {
        public Guid? TerminalID { get; set; }

        public Guid MerchantID { get; set; }

        public OperationCodesEnum OperationCode { get; set; }

        public string OperationDescription { get; set; }
    }
}
