using Microsoft.EntityFrameworkCore;
using Shared.Helpers;
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

            // TODO: we can try to transmit transactions which are failed to transmit
            if (filter.NotTransmitted)
            {
                src = src.Where(t => t.Status == Shared.Enums.TransactionStatusEnum.CommitedByAggregator);
            }
            else if (filter.QuickStatusFilter != null)
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

            if (filter.TransactionType != null)
            {
                src = src.Where(t => t.TransactionType == filter.TransactionType);
            }

            if (filter.SpecialTransactionType != null)
            {
                src = src.Where(t => t.SpecialTransactionType == filter.SpecialTransactionType);
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

            if (filter.ConsumerID != null)
            {
                src = src.Where(t => t.DealDetails.ConsumerID == filter.ConsumerID);
            }

            if (!string.IsNullOrWhiteSpace(filter.CardNumber))
            {
                src = src.Where(t => EF.Functions.Like(t.CreditCardDetails.CardNumber, filter.CardNumber.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.ConsumerEmail))
            {
                src = src.Where(t => EF.Functions.Like(t.DealDetails.ConsumerEmail, filter.ConsumerEmail.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.CardOwnerName))
            {
                src = src.Where(t => EF.Functions.Like(t.CreditCardDetails.CardOwnerName, filter.CardOwnerName.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.CardOwnerNationalID))
            {
                src = src.Where(t => EF.Functions.Like(t.CreditCardDetails.CardOwnerNationalID, filter.CardOwnerNationalID.UseWildCard(true)));
            }

            if (filter.CreditCardTokenID != null)
            {
                src = src.Where(t => t.CreditCardToken == filter.CreditCardTokenID);
            }

            if (filter.BillingDealID != null)
            {
                src = src.Where(t => t.BillingDealID == filter.BillingDealID);
            }

            if (!string.IsNullOrWhiteSpace(filter.DealDescription))
            {
                src = src.Where(t => EF.Functions.Like(t.DealDetails.DealDescription, filter.DealDescription.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.DealReference))
            {
                src = src.Where(t => t.DealDetails.DealReference == filter.DealReference);
            }

            if (filter.Solek != null)
            {
                src = src.Where(t => t.ShvaTransactionDetails.Solek == filter.Solek);
            }

            if (!string.IsNullOrWhiteSpace(filter.CreditCardVendor))
            {
                src = src.Where(t => t.CreditCardDetails.CardVendor == filter.CreditCardVendor);
            }

            return src;
        }

        public static IQueryable<PaymentTransaction> Filter(this IQueryable<PaymentTransaction> src, TransmissionFilter filter)
        {
            if (filter.TerminalID != null)
            {
                src = src.Where(t => t.TerminalID == filter.TerminalID);
            }

            return src;
        }

        private static IQueryable<PaymentTransaction> HandleDateFiltering(IQueryable<PaymentTransaction> src, TransactionsFilter filter)
        {
            //TODO: Quick time filters using SequentialGuid https://stackoverflow.com/questions/54920200/entity-framework-core-guid-greater-than-for-paging
            if (filter.QuickDateFilter != null)
            {
                var dateTime = CommonFiltertingExtensions.QuickDateToDateTime(filter.QuickDateFilter.Value);

                if (filter.DateType == DateFilterTypeEnum.Created)
                {
                    src = src.Where(t => t.TransactionTimestamp >= dateTime);
                }
                else if (filter.DateType == DateFilterTypeEnum.Updated)
                {
                    src = src.Where(t => t.UpdatedDate >= dateTime);
                }
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
                        src = src.Where(t => t.TransactionTimestamp <= filter.DateTo.Value);
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
