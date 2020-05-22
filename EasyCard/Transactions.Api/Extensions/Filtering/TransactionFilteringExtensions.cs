using System;
using System.Linq;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Models.Transactions.Enums;
using Transactions.Business.Entities;

namespace Transactions.Api.Extensions.Filtering
{
    public static class TransactionFilteringExtensions
    {
        public static IQueryable<PaymentTransaction> Filter(this IQueryable<PaymentTransaction> src, TransactionsFilter filter)
        {
            if (filter.PaymentTransactionID != null)
            {
                src = src.Where(t => t.PaymentTransactionID == filter.PaymentTransactionID);
                return src;
            }

            if (filter.TerminalID != null)
            {
                src = src.Where(t => t.TerminalID == filter.TerminalID);
            }

            if (filter.MerchantID != null)
            {
                src = src.Where(t => t.MerchantID == filter.MerchantID);
            }

            if (filter.AmountFrom != null && filter.AmountFrom > 0)
            {
                src = src.Where(t => t.TransactionAmount >= filter.AmountFrom);
            }

            if (filter.AmountTo != null && filter.AmountTo > 0)
            {
                src = src.Where(t => t.TransactionAmount <= filter.AmountTo);
            }

            if (filter.Currency != null)
            {
                src = src.Where(t => t.Currency == filter.Currency);
            }

            src = HandleDateFiltering(src, filter);

            if (filter.QuickStatusFilter != null)
            {
                src = FilterByQuickStatus(src, filter.QuickStatusFilter.Value);
            }
            else if (filter.Statuses != null && filter.Statuses.Count > 0)
            {
                src = src.Where(t => filter.Statuses.Contains(t.Status));
            }

            if (filter.JDealType != null)
            {
                src = src.Where(t => t.JDealType == filter.JDealType);
            }

            if (filter.Type != null)
            {
                src = src.Where(t => t.TransactionType == filter.Type);
            }

            if (filter.CardPresence != null)
            {
                src = src.Where(t => t.CardPresence == filter.CardPresence);
            }

            if (!string.IsNullOrEmpty(filter.ShvaShovarNumber))
            {
                src = src.Where(t => t.ShvaTransactionDetails.ShvaShovarNumber == filter.ShvaShovarNumber);
            }

            if (!string.IsNullOrEmpty(filter.ShvaTransactionID))
            {
                src = src.Where(t => t.ShvaTransactionDetails.ShvaDealID == filter.ShvaTransactionID);
            }

            if (filter.ClearingHouseTransactionID != null)
            {
                src = src.Where(t => t.ClearingHouseTransactionDetails.ClearingHouseTransactionID == filter.ClearingHouseTransactionID);
            }

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
                QuickStatusFilterTypeEnum.Completed => src.Where(t => t.Status == Shared.Enums.TransactionStatusEnum.TransmittedToProcessor),
                QuickStatusFilterTypeEnum.Failed => src.Where(t => (int)t.Status < 0),
                _ => src,
            };
    }
}
