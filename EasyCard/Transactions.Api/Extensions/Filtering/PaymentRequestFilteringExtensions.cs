using Shared.Api.Extensions.Filtering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Invoicing;
using Transactions.Api.Models.PaymentRequests;
using Transactions.Api.Models.PaymentRequests.Enums;
using Transactions.Business.Entities;

namespace Transactions.Api.Extensions.Filtering
{
    public static class PaymentRequestFilteringExtensions
    {
        public static IQueryable<PaymentRequest> Filter(this IQueryable<PaymentRequest> src, PaymentRequestsFilter filter)
        {
            if (filter.PaymentRequestID != null)
            {
                src = src.Where(t => t.PaymentRequestID == filter.PaymentRequestID);
                return src;
            }

            if (filter.TerminalID != null)
            {
                src = src.Where(t => t.TerminalID == filter.TerminalID);
            }

            if (filter.Currency != null)
            {
                src = src.Where(t => t.Currency == filter.Currency);
            }

            // TODO: date filtering for payment request
            if (filter.QuickDateFilter != null)
            {
                var dateRange = CommonFiltertingExtensions.QuickDateToDateRange(filter.QuickDateFilter.Value);

                src = src.Where(t => t.DueDate >= dateRange.DateFrom && t.DueDate <= dateRange.DateTo);
            }
            else
            {
                if (filter.DateFrom != null)
                {
                    src = src.Where(t => t.DueDate >= filter.DateFrom.Value);
                }

                if (filter.DateTo != null)
                {
                    src = src.Where(t => t.DueDate <= filter.DateTo.Value);
                }
            }

            if (filter.Status != null)
            {
                src = src.Where(t => t.Status == filter.Status);
            }
            else if (filter.QuickStatus != null)
            {
                src = FilterByQuickStatus(src, filter.QuickStatus.Value);
            }

            if (filter.ConsumerID != null)
            {
                src = src.Where(t => t.DealDetails.ConsumerID == filter.ConsumerID);
            }

            if (filter.ConsumerExternalReference != null)
            {
                src = src.Where(t => t.DealDetails.ConsumerExternalReference == filter.ConsumerExternalReference);
            }

            if (filter.PaymentRequestAmount > 0)
            {
                src = src.Where(t => t.PaymentRequestAmount == filter.PaymentRequestAmount);
            }

            return src;
        }

        private static IQueryable<PaymentRequest> FilterByQuickStatus(IQueryable<PaymentRequest> src, PayReqQuickStatusFilterTypeEnum typeEnum)
            => typeEnum switch
            {
                PayReqQuickStatusFilterTypeEnum.Pending => src.Where(t => (int)t.Status >= 1 && (int)t.Status < 4),
                PayReqQuickStatusFilterTypeEnum.Completed => src.Where(t => t.Status == Shared.Enums.PaymentRequestStatusEnum.Payed),
                PayReqQuickStatusFilterTypeEnum.Canceled => src.Where(t => t.Status == Shared.Enums.PaymentRequestStatusEnum.Canceled || t.Status == Shared.Enums.PaymentRequestStatusEnum.Rejected),
                PayReqQuickStatusFilterTypeEnum.Overdue => src.Where(t => t.DueDate.HasValue && t.DueDate < DateTime.UtcNow),
                PayReqQuickStatusFilterTypeEnum.Failed => src.Where(t => (int)t.Status < 0),
                _ => src,
            };
    }
}
