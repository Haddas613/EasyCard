using Shared.Api.Models;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.PaymentRequests.Enums;
using Transactions.Api.Models.Transactions.Enums;
using Transactions.Shared.Enums;

namespace Transactions.Api.Models.PaymentRequests
{
    public class PaymentRequestsFilter : FilterBase
    {
        public Guid? TerminalID { get; set; }

        public Guid? PaymentRequestID { get; set; }

        public CurrencyEnum? Currency { get; set; }

        public QuickDateFilterTypeEnum? QuickDateFilter { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public DateFilterTypeEnum? DateType { get; set; }

        //public PaymentRequestStatusEnum? Status { get; set; }

        public PayReqQuickStatusFilterTypeEnum? QuickStatus { get; set; }

        public decimal? PaymentRequestAmount { get; set; }

        public Guid? ConsumerID { get; set; }
    }
}
