using Merchants.Api.Models.Terminal;
using Merchants.Api.Models.TerminalTemplate;
using Merchants.Business.Entities.Terminal;
using Microsoft.EntityFrameworkCore;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Extensions.Filtering
{
    public static class TerminalTemplateFilteringExtensions
    {
        public static IQueryable<TerminalTemplate> Filter(this IQueryable<TerminalTemplate> src, TerminalTemplatesFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.Label))
            {
                src = src.Where(t => EF.Functions.Like(t.Label, filter.Label.UseWildCard(true)));
            }

            return src;
        }
    }
}
