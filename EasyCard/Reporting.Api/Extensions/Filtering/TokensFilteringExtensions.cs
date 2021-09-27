using Microsoft.EntityFrameworkCore;
using Reporting.Api.Models.Tokens;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Business.Entities;

namespace Reporting.Api.Extensions.Filtering
{
    public static class TokensFilteringExtensions
    {
        public static IQueryable<CreditCardTokenDetails> Filter(this IQueryable<CreditCardTokenDetails> src, TokensTransactionsFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.CardNumber))
            {
                src = src.Where(t => EF.Functions.Like(t.CardNumber, filter.CardNumber.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.ConsumerEmail))
            {
                src = src.Where(t => EF.Functions.Like(t.ConsumerEmail, filter.ConsumerEmail.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.CardOwnerName))
            {
                src = src.Where(t => EF.Functions.Like(t.CardOwnerName, filter.CardOwnerName.UseWildCard(true)));
            }

            if (filter.ConsumerID != null)
            {
                src = src.Where(t => t.ConsumerID == filter.ConsumerID);
            }

            if (!string.IsNullOrWhiteSpace(filter.CardOwnerNationalID))
            {
                src = src.Where(t => EF.Functions.Like(t.CardOwnerNationalID, filter.CardOwnerNationalID.UseWildCard(true)));
            }

            if (filter.CreditCardTokenID != null)
            {
                src = src.Where(t => t.CreditCardTokenID == filter.CreditCardTokenID);
            }

            if (filter.TerminalID != null)
            {
                src = src.Where(t => t.TerminalID == filter.TerminalID);
            }

            if (filter.MerchantID != null)
            {
                src = src.Where(t => t.MerchantID == filter.MerchantID);
            }

            return src;
        }
    }
}
