using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Merchant
{
    public class MerchantHistory
    {
        public long MerchantHistoryID { get; set; }

        public long? MerchantID { get; set; }

        public virtual Merchant Merchant { get; set; }

        public long? TerminalID { get; set; }

        public virtual Terminal.Terminal Terminal { get; set; }

        public DateTime? OperationDate { get; set; }

        public string OperationCode { get; set; }

        public string OperationDoneBy { get; set; }

        public string OperationDoneByID { get; set; }

        public string OperationDescription { get; set; }

        public string AdditionalDetails { get; set; }

        public string CorrelationId { get; set; }

        public string SourceIP { get; set; }

        public string ReasonForChange { get; set; }
    }
}
