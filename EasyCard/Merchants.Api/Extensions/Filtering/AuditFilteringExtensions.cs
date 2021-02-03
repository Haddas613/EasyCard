using Merchants.Api.Models.Audit;
using Merchants.Business.Entities.Merchant;
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

            if (filter.TerminalID.HasValue)
            {
                src = src.Where(h => h.TerminalID == filter.TerminalID.Value);
            }

            if (filter.UserID.HasValue)
            {
                src = src.Where(h => h.OperationDoneByID == filter.UserID.Value);
            }

            return src;
        }
    }
}
