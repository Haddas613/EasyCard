using MerchantProfileApi.Models.Terminal;
using Merchants.Business.Entities.Terminal;
using Microsoft.EntityFrameworkCore;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Extensions
{
    public static class TerminalFilteringExtensions
    {
        public static IQueryable<Terminal> Filter(this IQueryable<Terminal> src, TerminalsFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.Label))
            {
                src = src.Where(t => EF.Functions.Like(t.Label, filter.Label.UseWildCard(true)));
            }

            return src;
        }
    }
}
