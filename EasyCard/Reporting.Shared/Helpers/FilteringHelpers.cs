using Reporting.Shared.Models;
using Shared.Api.Extensions.Filtering;
using Shared.Api.Models.Enums;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reporting.Shared.Helpers
{
    public static class FilteringHelpers
    {
        public static void NormalizeFilter(this DashboardQuery request)
        {
            var useDateRange = false;

            // If not enough info
            if (!request.QuickDateFilter.HasValue || request.DateTo.HasValue || request.DateFrom.HasValue)
            {
                if (request.DateTo == null)
                {
                    request.DateTo = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;
                }

                if (request.DateFrom == null)
                {
                    request.DateFrom = request.DateTo.Value.AddDays(-30).Date;
                }

                if (!request.Granularity.HasValue)
                {
                    request.Granularity = CommonFiltertingExtensions.GetReportGranularity(request.DateFrom.Value, request.DateTo.Value);
                }
            }
            else
            {
                useDateRange = true;
                var dateRange = CommonFiltertingExtensions.QuickDateToDateRange(request.QuickDateFilter.Value);
                request.DateFrom = dateRange.DateFrom;
                request.DateTo = dateRange.DateTo;

                if (!request.Granularity.HasValue)
                {
                    request.Granularity = CommonFiltertingExtensions.GetReportGranularity(request.QuickDateFilter.Value);
                }
            }

            if (request.AltQuickDateFilter != QuickDateFilterAltEnum.NoComparison && (request.AltQuickDateFilter.HasValue || request.AltDateTo.HasValue || request.AltDateFrom.HasValue))
            {
                if (request.AltDateTo.HasValue || request.AltDateFrom.HasValue || (request.AltQuickDateFilter.Value == QuickDateFilterAltEnum.PrevPeriod && !useDateRange))
                {
                    if (request.AltDateTo == null)
                    {
                        request.AltDateTo = request.DateFrom.Value.AddDays(-1);
                    }

                    if (request.AltDateFrom == null)
                    {
                        request.AltDateFrom = request.AltDateTo.Value.AddDays(-(request.DateTo.Value - request.DateFrom.Value).TotalDays).Date;
                    }
                }
                else if (request.AltQuickDateFilter.Value == QuickDateFilterAltEnum.PrevPeriod && useDateRange)
                {
                    var dateRange = CommonFiltertingExtensions.QuickDateToPrevDateRange(request.QuickDateFilter.Value);
                    request.AltDateFrom = dateRange.DateFrom;
                    request.AltDateTo = dateRange.DateTo;
                }
                else
                {
                    var dateRange = CommonFiltertingExtensions.AltQuickDateToDateRange(request.AltQuickDateFilter.Value);
                    request.AltDateFrom = dateRange.DateFrom;
                    request.AltDateTo = dateRange.DateTo;
                }
            }
        }
    }
}
