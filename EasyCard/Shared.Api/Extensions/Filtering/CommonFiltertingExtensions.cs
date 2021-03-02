using Shared.Api.Models.Enums;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Api.Extensions.Filtering
{
    public class CommonFiltertingExtensions
    {
        public static DateTime QuickTimeToDateTime(QuickTimeFilterTypeEnum typeEnum)
            => typeEnum switch
            {
                QuickTimeFilterTypeEnum.Last5Minutes => DateTime.UtcNow.AddMinutes(-5),
                QuickTimeFilterTypeEnum.Last15Minutes => DateTime.UtcNow.AddMinutes(-15),
                QuickTimeFilterTypeEnum.Last30Minutes => DateTime.UtcNow.AddMinutes(-30),
                QuickTimeFilterTypeEnum.LastHour => DateTime.UtcNow.AddHours(-1),
                QuickTimeFilterTypeEnum.Last24Hours => DateTime.UtcNow.AddHours(-24),
                _ => DateTime.MinValue,
            };

        //Today, Yesterday, This week, last week, last 30 days, this month, prev month and custom.
        //last month, or last 3 months or custom
        public static Tuple<DateTime, DateTime> QuickDateToDateRange(QuickDateFilterTypeEnum typeEnum)
        {
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;
            var yesterday = today.AddDays(-1);
            var thisSunday = today.AddDays(-(int)today.DayOfWeek);
            var prevSunday = thisSunday.AddDays(-7);
            var thisMonthStart = today.AddDays(-today.Day);
            var prevMonthStart = thisMonthStart.AddMonths(-1);
            var prevPrevMonthStart = thisMonthStart.AddMonths(-2);

            return typeEnum switch
            {
                QuickDateFilterTypeEnum.Today => Tuple.Create(today, today),
                QuickDateFilterTypeEnum.Yesterday => Tuple.Create(yesterday, yesterday),
                QuickDateFilterTypeEnum.ThisWeek => Tuple.Create(thisSunday, thisSunday.AddDays(6)),
                QuickDateFilterTypeEnum.LastWeek => Tuple.Create(prevSunday, prevSunday.AddDays(6)),
                QuickDateFilterTypeEnum.Last30Days => Tuple.Create(today, today.AddDays(-30)),
                QuickDateFilterTypeEnum.ThisMonth => Tuple.Create(thisMonthStart, thisMonthStart.AddMonths(1).AddDays(-1)),
                QuickDateFilterTypeEnum.LastMonth => Tuple.Create(prevMonthStart, prevMonthStart.AddMonths(1).AddDays(-1)),
                QuickDateFilterTypeEnum.Last3Months => Tuple.Create(prevPrevMonthStart, prevMonthStart.AddMonths(3).AddDays(-1)),
                _ => Tuple.Create(today, today),
            };
        }

        public static ReportGranularityEnum GetReportGranularity(QuickDateFilterTypeEnum? typeEnum, DateTime? dateFrom, DateTime? dateTo, ReportGranularityEnum? granularity)
        {
            // TODO:
            return ReportGranularityEnum.Date;
        }
    }
}
