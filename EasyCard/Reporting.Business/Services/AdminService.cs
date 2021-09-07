using Reporting.Business.Models;
using Reporting.Shared.Models;
using Reporting.Shared.Models.Admin;
using Shared.Api.Extensions.Filtering;
using Shared.Api.Models.Enums;
using Shared.Helpers;
using Shared.Helpers.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Business.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAppInsightReaderService appInsightReaderService;

        public AdminService(IAppInsightReaderService appInsightReaderService)
        {
            this.appInsightReaderService = appInsightReaderService;
        }

        public async Task<AdminSmsTimelines> GetSmsTotals(DashboardQuery query)
        {
            //TODO: extract
            NormalizeFilter(query);

            var timestampFilter = KustoAgo(query.DateFrom.Value, query.DateTo.Value);
            var granularity = KustoGranularity(query.Granularity.Value);

            //TODO: SmsSent and SmsError
            var kustoQuery = @$"
                customEvents 
                | where timestamp > ago({timestampFilter})
                | where (name == ""ServiceProfilerIndex"" or name == ""ServiceProfilerSample"")
                | summarize count = count() by bin(timestamp, {granularity}), name
                | order by timestamp asc 
            ";

            var aiRes = await appInsightReaderService.GetData<SMSAppInsightsResult>(kustoQuery);

            var response = new AdminSmsTimelines
            {
                DateFrom = query.DateFrom,
                DateTo = query.DateTo,
                Granularity = query.Granularity
            };

            var all = aiRes.Select(e => new SmsTimeline {
                DimensionValue = e.Timestamp,
                //Type = e.Name == "SmsSent" ? SmsTimelineTypeEnum.Success : SmsTimelineTypeEnum.Error,
                Type = e.Name == "ServiceProfilerIndex" ? SmsTimelineTypeEnum.Success : SmsTimelineTypeEnum.Error, //TODO: remove
                Measure = e.Count
            });

            response.Success = all.Where(d => d.Type == SmsTimelineTypeEnum.Success);
            response.Error = all.Where(d => d.Type == SmsTimelineTypeEnum.Error);

            response.SuccessMeasure = response.Success.Sum(d => d.Measure);
            response.ErrorMeasure = response.Error.Sum(d => d.Measure);

            return response;
        }

        private string KustoAgo(DateTime from, DateTime to)
        {
            return $"{(to - from).TotalDays}d";
        }

        private string KustoGranularity(ReportGranularityEnum reportGranularity)
        {
            return reportGranularity switch
            {
                ReportGranularityEnum.Date => "1d",
                ReportGranularityEnum.Week => "7d",
                ReportGranularityEnum.Month => "30d",
                _ => "7d"
            };
        }

        private void NormalizeFilter(DashboardQuery request)
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
