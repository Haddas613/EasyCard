using Merchants.Api.Models.Terminal;
using Merchants.Business.Entities.Terminal;
using Microsoft.EntityFrameworkCore;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Extensions.Filtering
{
    public static class TerminalFilteringExtensions
    {
        public static IQueryable<Terminal> Filter(this IQueryable<Terminal> src, TerminalsFilter filter)
        {
            if (filter.TerminalID.HasValue)
            {
                return src.Where(t => t.TerminalID == filter.TerminalID.Value);
            }

            if (filter.MerchantID.HasValue)
            {
                src = src.Where(t => t.MerchantID == filter.MerchantID.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Label))
            {
                src = src.Where(t => EF.Functions.Like(t.Label, filter.Label.UseWildCard(true)));
            }

            if (filter.Status.HasValue)
            {
                src = src.Where(t => t.Status == filter.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.AggregatorTerminalReference))
            {
                src = src.Where(t => EF.Functions.Like(t.AggregatorTerminalReference, filter.AggregatorTerminalReference.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.ProcessorTerminalReference))
            {
                src = src.Where(t => EF.Functions.Like(t.ProcessorTerminalReference, filter.ProcessorTerminalReference.UseWildCard(true)));
            }

            return src;
        }
    }
}
