using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Shared.Enums;

namespace Transactions.Api.Models.PaymentRequests
{
    public class PaymentRequestHistorySummary
    {
        public Guid PaymentRequestHistoryID { get; set; }

        public DateTime? OperationDate { get; set; }

        public string OperationDoneBy { get; set; }

        public PaymentRequestOperationCodesEnum OperationCode { get; set; }

        public string OperationMessage { get; set; }
    }
}
