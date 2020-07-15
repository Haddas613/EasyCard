using System;
using System.Linq;
using Transactions.Api.Models.Billing;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Models.Transactions.Enums;
using Transactions.Business.Entities;

namespace Transactions.Api.Extensions.Filtering
{
    public static class BillingDealFilteringExtensions
    {
        public static IQueryable<BillingDeal> Filter(this IQueryable<BillingDeal> src, BillingDealsFilter filter)
        {
            return src;
        }

        private static IQueryable<PaymentTransaction> HandleDateFiltering(IQueryable<PaymentTransaction> src, TransactionsFilter filter)
        {
            //TODO: Quick time filters using SequentialGuid https://stackoverflow.com/questions/54920200/entity-framework-core-guid-greater-than-for-paging
            if (filter.QuickTimeFilter != null)
            {
                src = FilterByQuickTime(src, filter.QuickTimeFilter.Value);
            }
            else
            {
                if (filter.DateType == DateFilterTypeEnum.Created)
                {
                    if (filter.DateFrom != null)
                    {
                        src = src.Where(t => t.TransactionTimestamp >= filter.DateFrom.Value);
                    }

                    if (filter.DateTo != null)
                    {
                        src = src.Where(t => t.TransactionTimestamp <= filter.DateFrom.Value);
                    }
                }

                if (filter.DateType == DateFilterTypeEnum.Updated)
                {
                    if (filter.DateFrom != null)
                    {
                        src = src.Where(t => t.UpdatedDate >= filter.DateFrom.Value);
                    }

                    if (filter.DateTo != null)
                    {
                        src = src.Where(t => t.UpdatedDate <= filter.DateTo.Value);
                    }
                }
            }

            return src;
        }

        private static IQueryable<PaymentTransaction> FilterByQuickTime(IQueryable<PaymentTransaction> src, QuickTimeFilterTypeEnum typeEnum)
            => typeEnum switch
            {
                QuickTimeFilterTypeEnum.Last5Minutes => src.Where(t => t.UpdatedDate >= DateTime.UtcNow.AddMinutes(-5)),
                QuickTimeFilterTypeEnum.Last15Minutes => src.Where(t => t.UpdatedDate >= DateTime.UtcNow.AddMinutes(-15)),
                QuickTimeFilterTypeEnum.Last30Minutes => src.Where(t => t.UpdatedDate >= DateTime.UtcNow.AddMinutes(-30)),
                QuickTimeFilterTypeEnum.LastHour => src.Where(t => t.UpdatedDate >= DateTime.UtcNow.AddHours(-1)),
                QuickTimeFilterTypeEnum.Last24Hours => src.Where(t => t.UpdatedDate >= DateTime.UtcNow.AddHours(-24)),
                _ => src,
            };

        private static IQueryable<PaymentTransaction> FilterByQuickStatus(IQueryable<PaymentTransaction> src, QuickStatusFilterTypeEnum typeEnum)
            => typeEnum switch
            {
                QuickStatusFilterTypeEnum.Pending => src.Where(t => (int)t.Status > 0 && (int)t.Status < 40),
                QuickStatusFilterTypeEnum.Completed => src.Where(t => t.Status == Shared.Enums.TransactionStatusEnum.TransmittedByProcessor),
                QuickStatusFilterTypeEnum.Failed => src.Where(t => (int)t.Status < 0),
                _ => src,
            };
    }
}
