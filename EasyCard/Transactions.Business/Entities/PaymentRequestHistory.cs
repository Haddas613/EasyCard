using Shared.Business.Security;
using Shared.Helpers;
using System;
using Transactions.Shared.Enums;

namespace Transactions.Business.Entities
{
    public class PaymentRequestHistory : IAuditEntity
    {
        public PaymentRequestHistory()
        {
            OperationDate = DateTime.UtcNow;
            PaymentRequestHistoryID = Guid.NewGuid().GetSequentialGuid(OperationDate.Value);
        }

        public Guid PaymentRequestHistoryID { get; set; }

        public PaymentRequest PaymentRequest { get; set; }

        public Guid? PaymentRequestID { get; set; }

        public DateTime? OperationDate { get; set; }

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        public PaymentRequestOperationCodesEnum OperationCode { get; set; }

        public string OperationDescription { get; set; }

        public string OperationMessage { get; set; }

        public string CorrelationId { get; set; }

        public string SourceIP { get; set; }
    }
}