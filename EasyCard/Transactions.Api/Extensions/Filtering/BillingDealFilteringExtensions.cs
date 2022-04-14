using Microsoft.EntityFrameworkCore;
using Shared.Api.Extensions.Filtering;
using Shared.Helpers;
using System;
using System.Linq;
using Transactions.Api.Models.Billing;
using Transactions.Api.Models.Transactions;
using Transactions.Business.Entities;
using SharedHelpers = Shared.Helpers;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Extensions.Filtering
{
    public static class BillingDealFilteringExtensions
    {
        public static IQueryable<BillingDeal> Filter(this IQueryable<BillingDeal> src, BillingDealsFilter filter)
        {
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;

            if (filter.QuickStatus != null)
            {
                return src.FilterByQuickStatus(filter.QuickStatus.GetValueOrDefault(), today);
            }

            if (filter.BillingDealID != null)
            {
                src = src.Where(t => t.BillingDealID == filter.BillingDealID);
                return src;
            }

            if (filter.ShowDeleted == SharedHelpers.Models.ShowDeletedEnum.OnlyDeleted || filter.Finished == true)
            {
                src = src.Where(t => t.Active == false);
            }
            else
            {
                src = src.Where(t => t.Active == true);
            }

            if (filter.Finished == true)
            {
                src = src.Where(t => t.NextScheduledTransaction == null);
            }

            if (filter.Actual)
            {
                src = src
                    .Where(t => t.InProgress == Shared.Enums.BillingProcessingStatusEnum.Pending && t.Active && t.NextScheduledTransaction != null && t.NextScheduledTransaction.Value.Date <= today)
                    .Where(t => (t.PausedFrom == null || t.PausedFrom > today) && (t.PausedTo == null || t.PausedTo < today));
            }

            if (filter.InProgress)
            {
                src = src
                    .Where(t => t.InProgress != Shared.Enums.BillingProcessingStatusEnum.Pending);
            }

            if (filter.OnlyActive == true)
            {
                src = src.Where(t => t.InProgress == Shared.Enums.BillingProcessingStatusEnum.Pending && t.Active && t.NextScheduledTransaction != null);
            }

            if (filter.HasError == true)
            {
                src = src.Where(t => t.HasError);
            }

            if (filter.Paused == true)
            {
                src = src.Where(t => (t.PausedFrom != null || t.PausedFrom >= today) && (t.PausedTo != null || t.PausedTo >= today));
            }

            if (filter.InvoiceOnly)
            {
                src = src.Where(t => t.InvoiceOnly);
            }

            if (filter.PaymentType != null)
            {
                src = src.Where(t => t.PaymentType == filter.PaymentType);
            }

            if (filter.TerminalID != null)
            {
                src = src.Where(t => t.TerminalID == filter.TerminalID);
            }

            if (filter.MerchantID != null)
            {
                src = src.Where(t => t.MerchantID == filter.MerchantID);
            }

            if (filter.Currency != null)
            {
                src = src.Where(t => t.Currency == filter.Currency);
            }

            if (filter.CreditCardExpired)
            {
                return src.FilterCardExpired(today);
            }

            src = HandleDateFiltering(src, filter);

            if (filter.ConsumerID != null)
            {
                src = src.Where(t => t.DealDetails.ConsumerID == filter.ConsumerID);
            }

            if (filter.ConsumerExternalReference != null)
            {
                src = src.Where(t => t.DealDetails.ConsumerExternalReference == filter.ConsumerExternalReference);
            }

            if (!string.IsNullOrWhiteSpace(filter.CardNumber))
            {
                src = src.Where(t => EF.Functions.Like(t.CreditCardDetails.CardNumber, filter.CardNumber.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.ConsumerEmail))
            {
                src = src.Where(t => EF.Functions.Like(t.DealDetails.ConsumerEmail, filter.ConsumerEmail.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.ConsumerName))
            {
                src = src.Where(t => EF.Functions.Like(t.CreditCardDetails.CardOwnerName, filter.ConsumerName.UseWildCard(true))
                    || EF.Functions.Like(t.DealDetails.ConsumerName, filter.ConsumerName.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.CardOwnerNationalID))
            {
                src = src.Where(t => EF.Functions.Like(t.CreditCardDetails.CardOwnerNationalID, filter.CardOwnerNationalID.UseWildCard(true)));
            }

            if (filter.CreditCardTokenID != null)
            {
                src = src.Where(t => t.CreditCardToken == filter.CreditCardTokenID);
            }

            if (!string.IsNullOrWhiteSpace(filter.CreditCardVendor))
            {
                src = src.Where(t => t.CreditCardDetails.CardVendor == filter.CreditCardVendor);
            }

            if (!string.IsNullOrWhiteSpace(filter.DealReference))
            {
                src = src.Where(t => t.DealDetails.DealReference == filter.DealReference);
            }

            if (!string.IsNullOrWhiteSpace(filter.Origin))
            {
                src = src.Where(t => t.Origin == filter.Origin);
            }

            return src;
        }

        public static IQueryable<BillingDeal> FilterByQuickStatus(this IQueryable<BillingDeal> src, BillingsQuickStatusFilterEnum quickStatus, DateTime today)
        {
            switch (quickStatus)
            {
                case BillingsQuickStatusFilterEnum.Completed:
                    return src.Where(t => t.NextScheduledTransaction == null);

                case BillingsQuickStatusFilterEnum.Failed:
                    return src.Where(t => t.Active && t.HasError);

                case BillingsQuickStatusFilterEnum.Inactive:
                    return src.Where(t => t.Active == false);

                case BillingsQuickStatusFilterEnum.Paused:
                    return src.Where(t =>
                        t.Active && (t.PausedFrom != null || t.PausedFrom >= today) && (t.PausedTo != null || t.PausedTo >= today));

                case BillingsQuickStatusFilterEnum.ManualTrigger:
                    return src.Where(t =>
                        t.InProgress == Shared.Enums.BillingProcessingStatusEnum.Pending && t.Active &&
                        t.NextScheduledTransaction != null && t.NextScheduledTransaction.Value.Date <= today)
                    .Where(t => (t.PausedFrom == null || t.PausedFrom > today) && (t.PausedTo == null || t.PausedTo < today));

                case BillingsQuickStatusFilterEnum.TriggeredTomorrow:
                    return src.Where(t =>
                        t.Active &&
                        t.NextScheduledTransaction != null && t.NextScheduledTransaction.Value.Date == today.AddDays(1))
                    .Where(t => (t.PausedFrom == null || t.PausedFrom > today) && (t.PausedTo == null || t.PausedTo < today));

                case BillingsQuickStatusFilterEnum.CardExpired:
                    return src.FilterCardExpired(today);

                case BillingsQuickStatusFilterEnum.ExpiredNextMonth:
                    var lastDayOfNextMonth = today.AddMonths(1).LastDayOfMonth();
                    return src.Where(t => t.Active && t.CreditCardDetails.ExpirationDate == lastDayOfNextMonth);

                default:
                    return src.Where(t => t.Active);
            }
        }

        public static IQueryable<BillingDeal> FilterCardExpired(this IQueryable<BillingDeal> src, DateTime? today = null)
        {
            if (today == null)
            {
                today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;
            }

            var lastDayOfMonth = today.Value.LastDayOfMonth();
            return src.Where(t => t.Active && t.PaymentType == SharedIntegration.Models.PaymentTypeEnum.Card && t.InvoiceOnly == false && (t.CreditCardDetails.ExpirationDate < lastDayOfMonth || t.CreditCardToken == null));
        }

        private static IQueryable<BillingDeal> HandleDateFiltering(IQueryable<BillingDeal> src, BillingDealsFilter filter)
        {
            // TODO: date filtering for billing deals
            if (filter.QuickDateFilter != null)
            {
                var dateRange = CommonFiltertingExtensions.QuickDateToDateRange(filter.QuickDateFilter.Value);

                src = src.Where(t => t.UpdatedDate >= dateRange.DateFrom && t.UpdatedDate <= dateRange.DateTo);
            }
            else
            {
                if (filter.DateFrom != null)
                {
                    src = filter.FilterDateByNextScheduledTransaction == true
                            ? src.Where(t => t.NextScheduledTransaction >= filter.DateFrom.Value)
                            : src.Where(t => t.BillingDealTimestamp >= filter.DateFrom.Value);
                }

                if (filter.DateTo != null)
                {
                    src = filter.FilterDateByNextScheduledTransaction == true
                            ? src.Where(t => t.NextScheduledTransaction <= filter.DateTo.Value)
                            : src.Where(t => t.BillingDealTimestamp <= filter.DateTo.Value);
                }
            }

            return src;
        }
    }
}
