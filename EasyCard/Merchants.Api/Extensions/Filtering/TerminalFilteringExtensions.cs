using Merchants.Api.Models.Terminal;
using Merchants.Business.Entities.Terminal;
using Microsoft.EntityFrameworkCore;
using Shared.Helpers;
using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedApi = Shared.Api;

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

            if (filter.TerminalTemplateID.HasValue)
            {
                src = src.Where(t => t.TerminalTemplateID.Value == filter.TerminalTemplateID.Value);
            }

            if (filter.MerchantID.HasValue)
            {
                src = src.Where(t => t.MerchantID == filter.MerchantID.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Label))
            {
                src = src.Where(t => EF.Functions.Like(t.Label, filter.Label.UseWildCard(true)));
            }

            if (filter.DateType == SharedApi.Models.Enums.DateFilterTypeEnum.Updated)
            {
                if (filter.DateFrom != null)
                {
                    src = src.Where(t => t.Updated >= filter.DateFrom.Value);
                }

                if (filter.DateTo != null)
                {
                    src = src.Where(t => t.Updated <= filter.DateTo.Value);
                }
            }
            else
            {
                if (filter.DateFrom != null)
                {
                    src = src.Where(t => t.Created >= filter.DateFrom.Value);
                }

                if (filter.DateTo != null)
                {
                    src = src.Where(t => t.Created <= filter.DateTo.Value);
                }
            }

            if (filter.Status.HasValue)
            {
                src = src.Where(t => t.Status == filter.Status.Value);
            }
            else if (filter.ActiveOnly)
            {
                src = src.Where(t => t.Status != Shared.Enums.TerminalStatusEnum.Disabled);
            }

            if (!string.IsNullOrWhiteSpace(filter.AggregatorTerminalReference))
            {
                src = src.Where(t => EF.Functions.Like(t.AggregatorTerminalReference, filter.AggregatorTerminalReference.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.ProcessorTerminalReference))
            {
                src = src.Where(t => EF.Functions.Like(t.ProcessorTerminalReference, filter.ProcessorTerminalReference.UseWildCard(true)));
            }

            if (filter.HasShvaTerminal)
            {
                src = src.Where(t => t.Integrations.Any(t => t.ExternalSystemID == ExternalSystemHelpers.ShvaExternalSystemID));
            }

            return src;
        }
    }
}
