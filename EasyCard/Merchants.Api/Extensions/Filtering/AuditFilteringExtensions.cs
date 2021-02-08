using Merchants.Api.Models.Audit;
using Merchants.Business.Entities.Merchant;
using Microsoft.EntityFrameworkCore;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Extensions.Filtering
{
    public static class AuditFilteringExtensions
    {
        public static IQueryable<MerchantHistory> Filter(this IQueryable<MerchantHistory> src, AuditFilter filter)
        {
            if (filter.MerchantID.HasValue)
            {
                src = src.Where(h => h.MerchantID == filter.MerchantID.Value);
            }

            if (!string.IsNullOrEmpty(filter.MerchantName))
            {
                src = src.Where(h => EF.Functions.Like(h.Merchant.BusinessName, filter.MerchantName.UseWildCard(true)));
            }

            if (filter.TerminalID.HasValue)
            {
                src = src.Where(h => h.TerminalID == filter.TerminalID.Value);
            }

            if (!string.IsNullOrEmpty(filter.TerminalName))
            {
                src = src.Where(h => EF.Functions.Like(h.Terminal.Label, filter.TerminalName.UseWildCard(true)));
            }

            if (filter.UserID.HasValue)
            {
                src = src.Where(h => h.OperationDoneByID == filter.UserID.Value);
            }

            if (!string.IsNullOrEmpty(filter.UserName))
            {
                src = src.Where(h => EF.Functions.Like(h.OperationDoneBy, filter.UserName.UseWildCard(true)));
            }

            if (filter.Code.HasValue)
            {
                src = src.Where(h => h.OperationCode == filter.Code.Value);
            }

            if (filter.From.HasValue)
            {
                src = src.Where(h => h.OperationDate >= filter.From);
            }

            if (filter.To.HasValue)
            {
                var to = filter.To.Value.AddDays(1);
                src = src.Where(h => h.OperationDate < to);
            }

            return src;
        }
    }
}
