using Shared.Business.Security;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Shared.Enums;

namespace Transactions.Business.Entities
{
    public class BillingDealHistory : IAuditEntity
    {
        public BillingDealHistory()
        {
            OperationDate = DateTime.UtcNow;
            BillingDealHistoryID = Guid.NewGuid().GetSequentialGuid(OperationDate.Value);
        }

        public Guid BillingDealHistoryID { get; set; }

        public BillingDeal BillingDeal { get; set; }

        public Guid? BillingDealID { get; set; }

        public DateTime? OperationDate { get; set; }

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        public BillingDealOperationCodesEnum OperationCode { get; set; }

        public string OperationDescription { get; set; }

        public string OperationMessage { get; set; }

        public string CorrelationId { get; set; }

        public string SourceIP { get; set; }
    }
}
