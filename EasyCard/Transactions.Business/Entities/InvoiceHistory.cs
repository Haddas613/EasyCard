using Shared.Business.Security;
using Shared.Helpers;
using System;
using Transactions.Shared.Enums;

namespace Transactions.Business.Entities
{
    public class InvoiceHistory : IAuditEntity
    {
        public InvoiceHistory()
        {
            OperationDate = DateTime.UtcNow;
            InvoiceHistoryID = Guid.NewGuid().GetSequentialGuid(OperationDate.Value);
        }

        public Guid InvoiceHistoryID { get; set; }

        public Invoice Invoice { get; set; }

        public Guid? InvoiceID { get; set; }

        public DateTime? OperationDate { get; set; }

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        public InvoiceOperationCodesEnum OperationCode { get; set; }

        public string OperationDescription { get; set; }

        public string OperationMessage { get; set; }

        public string CorrelationId { get; set; }

        public string SourceIP { get; set; }
    }
}