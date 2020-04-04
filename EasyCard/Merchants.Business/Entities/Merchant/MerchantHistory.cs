using Shared.Business.Audit;
using Shared.Business.Security;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Merchant
{
    public class MerchantHistory : IAuditEntity
    {
        public MerchantHistory()
        {
            //Terminals = new HashSet<Merchants.Business.Entities.Terminal.Terminal>();
            OperationDate = DateTime.UtcNow;
            MerchantHistoryID = Guid.NewGuid().GetSequentialGuid(OperationDate.Value);
        }

        public Guid MerchantHistoryID { get; set; }

        public Guid? MerchantID { get; set; }

        public virtual Merchant Merchant { get; set; }

        public Guid? TerminalID { get; set; }

        public virtual Terminal.Terminal Terminal { get; set; }

        public DateTime? OperationDate { get; set; }

        public OperationCodesEnum OperationCode { get; set; }

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        public string OperationDescription { get; set; }

        public string AdditionalDetails { get; set; }

        public string CorrelationId { get; set; }

        public string SourceIP { get; set; }

        public string ReasonForChange { get; set; }
    }
}
