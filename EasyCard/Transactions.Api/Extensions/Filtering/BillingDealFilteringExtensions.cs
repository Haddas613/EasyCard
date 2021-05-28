using Microsoft.EntityFrameworkCore;
using Shared.Api.Extensions.Filtering;
using Shared.Helpers;
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
            if (filter.BillingDealID != null)
            {
                src = src.Where(t => t.BillingDealID == filter.BillingDealID);
                return src;
            }

            if (filter.Actual)
            {
                src = src
                    .Where(t => t.Active && t.NextScheduledTransaction != null && t.NextScheduledTransaction.Value.Date <= DateTime.Today)
                    .Where(t => (t.PausedFrom == null || t.PausedFrom > DateTime.Today) && (t.PausedTo == null || t.PausedTo < DateTime.Today));
            }
            else if (filter.Finished == true)
            {
                src = src.Where(t => t.NextScheduledTransaction == null);
            }
            else if (filter.ShowDeleted == true)
            {
                src = src.Where(t => t.Active == false);
            }
            else if (filter.Paused == true)
            {
                src = src.Where(t => (t.PausedFrom != null || t.PausedFrom >= DateTime.Today) && (t.PausedTo != null || t.PausedTo >= DateTime.Today));
            }
            else
            {
                src = src.Where(t => t.Active == true);
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

            src = HandleDateFiltering(src, filter);

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

            if (!string.IsNullOrWhiteSpace(filter.CreditCardVendor))
            {
                src = src.Where(t => t.CreditCardDetails.CardVendor == filter.CreditCardVendor);
            }

            return src;
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
                    src = src.Where(t => t.BillingDealTimestamp >= filter.DateFrom.Value);
                }

                if (filter.DateTo != null)
                {
                    src = src.Where(t => t.BillingDealTimestamp <= filter.DateTo.Value);
                }
            }

            return src;
        }
    }
}
