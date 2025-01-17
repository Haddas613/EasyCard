﻿using Microsoft.EntityFrameworkCore;
using Shared.Api.Extensions.Filtering;
using Shared.Helpers;
using System;
using System.Linq;
using Transactions.Api.Models.Billing;
using Transactions.Api.Models.Transactions;
using Transactions.Business.Entities;

namespace Transactions.Api.Extensions.Filtering
{
    public static class FutureBillingDealFilteringExtensions
    {
        public static IQueryable<FutureBilling> Filter(this IQueryable<FutureBilling> src, FutureBillingDealsFilter filter)
        {
            //if (filter.BillingDealID != null)
            //{
            //    src = src.Where(t => t.BillingDealID == filter.BillingDealID);
            //    return src;
            //}

            //if (filter.TerminalID != null)
            //{
            //    src = src.Where(t => t.TerminalID == filter.TerminalID);
            //}

            //if (filter.MerchantID != null)
            //{
            //    src = src.Where(t => t.MerchantID == filter.MerchantID);
            //}

            //if (filter.Currency != null)
            //{
            //    src = src.Where(t => t.Currency == filter.Currency);
            //}

            //src = HandleDateFiltering(src, filter);

            ////if (filter.ConsumerID != null)
            ////{
            ////    src = src.Where(t => t.DealDetails.ConsumerID == filter.ConsumerID);
            ////}

            //if (!string.IsNullOrWhiteSpace(filter.CardNumber))
            //{
            //    src = src.Where(t => EF.Functions.Like(t.CreditCardDetails.CardNumber, filter.CardNumber.UseWildCard(true)));
            //}

            ////if (!string.IsNullOrWhiteSpace(filter.ConsumerEmail))
            ////{
            ////    src = src.Where(t => EF.Functions.Like(t.DealDetails.ConsumerEmail, filter.ConsumerEmail.UseWildCard(true)));
            ////}

            //if (!string.IsNullOrWhiteSpace(filter.CardOwnerName))
            //{
            //    src = src.Where(t => EF.Functions.Like(t.CreditCardDetails.CardOwnerName, filter.CardOwnerName.UseWildCard(true)));
            //}

            //if (!string.IsNullOrWhiteSpace(filter.CardOwnerNationalID))
            //{
            //    src = src.Where(t => EF.Functions.Like(t.CreditCardDetails.CardOwnerNationalID, filter.CardOwnerNationalID.UseWildCard(true)));
            //}

            ////if (filter.CreditCardTokenID != null)
            ////{
            ////    src = src.Where(t => t.CreditCardToken == filter.CreditCardTokenID);
            ////}

            //if (!string.IsNullOrWhiteSpace(filter.CreditCardVendor))
            //{
            //    src = src.Where(t => t.CreditCardDetails.CardVendor == filter.CreditCardVendor);
            //}

            return src;
        }

        private static IQueryable<FutureBilling> HandleDateFiltering(IQueryable<FutureBilling> src, FutureBillingDealsFilter filter)
        {
            //if (filter.DateFrom != null)
            //{
            //    src = src.Where(t => t.FutureScheduledTransaction >= filter.DateFrom.Value);
            //}

            //if (filter.DateTo != null)
            //{
            //    src = src.Where(t => t.FutureScheduledTransaction <= filter.DateTo.Value);
            //}

            return src;
        }
    }
}
