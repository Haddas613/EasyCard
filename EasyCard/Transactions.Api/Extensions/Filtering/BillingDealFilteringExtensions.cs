using Microsoft.EntityFrameworkCore;
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
            //TODO: Quick time filters using SequentialGuid https://stackoverflow.com/questions/54920200/entity-framework-core-guid-greater-than-for-paging
            if (filter.QuickDateFilter != null)
            {
                var dateTime = CommonFiltertingExtensions.QuickDateToDateTime(filter.QuickDateFilter.Value);

                if (filter.DateType == DateFilterTypeEnum.Created)
                {
                    src = src.Where(t => t.BillingDealTimestamp >= dateTime);
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
                        src = src.Where(t => t.BillingDealTimestamp >= filter.DateFrom.Value);
                    }

                    if (filter.DateTo != null)
                    {
                        src = src.Where(t => t.BillingDealTimestamp <= filter.DateTo.Value);
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
    }
}
