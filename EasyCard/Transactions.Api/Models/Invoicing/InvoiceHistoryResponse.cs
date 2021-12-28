using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Shared.Enums;

namespace Transactions.Api.Models.Invoicing
{
    public class InvoiceHistoryResponse
    {
        public Guid InvoiceHistoryID { get; set; }

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
