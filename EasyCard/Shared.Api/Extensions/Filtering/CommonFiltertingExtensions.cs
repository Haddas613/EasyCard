using Shared.Api.Models.Enums;
using Shared.Helpers;
using Shared.Helpers.Models;
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
        public static DateRange QuickDateToDateRange(QuickDateFilterTypeEnum typeEnum)
        {
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;
            var yesterday = today.AddDays(-1);
            var thisSunday = today.AddDays(-(int)today.DayOfWeek);
            var prevSunday = thisSunday.AddDays(-7);
            var thisMonthStart = today.AddDays(-today.Day + 1);
            var prevMonthStart = thisMonthStart.AddMonths(-1);
            var prevPrevMonthStart = thisMonthStart.AddMonths(-2);

            return typeEnum switch
            {
                QuickDateFilterTypeEnum.Today => new DateRange(today, today.AddDays(1)),
                QuickDateFilterTypeEnum.Yesterday => new DateRange(yesterday, today),
                QuickDateFilterTypeEnum.ThisWeek => new DateRange(thisSunday, thisSunday.AddDays(6)),
                QuickDateFilterTypeEnum.LastWeek => new DateRange(prevSunday, prevSunday.AddDays(6)),
                QuickDateFilterTypeEnum.Last30Days => new DateRange(today.AddDays(-29), today),
                QuickDateFilterTypeEnum.ThisMonth => new DateRange(thisMonthStart, thisMonthStart.AddMonths(1).AddDays(-1)),
                QuickDateFilterTypeEnum.LastMonth => new DateRange(prevMonthStart, prevMonthStart.AddMonths(1).AddDays(-1)),
                QuickDateFilterTypeEnum.Last3Months => new DateRange(prevPrevMonthStart, prevPrevMonthStart.AddMonths(3).AddDays(-1)),
                _ => new DateRange(today, today),
            };
        }

        public static ReportGranularityEnum GetReportGranularity(QuickDateFilterTypeEnum? typeEnum)
        {
            return typeEnum switch
            {
                QuickDateFilterTypeEnum.Today => ReportGranularityEnum.Date,
                QuickDateFilterTypeEnum.Yesterday => ReportGranularityEnum.Date,
                QuickDateFilterTypeEnum.ThisWeek => ReportGranularityEnum.Date,
                QuickDateFilterTypeEnum.LastWeek => ReportGranularityEnum.Date,
                QuickDateFilterTypeEnum.Last30Days => ReportGranularityEnum.Week,
                QuickDateFilterTypeEnum.ThisMonth => ReportGranularityEnum.Week,
                QuickDateFilterTypeEnum.LastMonth => ReportGranularityEnum.Week,
                QuickDateFilterTypeEnum.Last3Months => ReportGranularityEnum.Month,
                _ => ReportGranularityEnum.Month
            };
        }

        public static ReportGranularityEnum GetReportGranularity(DateTime dateFrom, DateTime dateTo)
        {
            var dateRange = (dateTo - dateFrom).TotalDays;

            if (dateRange > 60)
            {
                return ReportGranularityEnum.Month;
            }
            else if (dateRange > 7)
            {
                return ReportGranularityEnum.Week;
            }
            else
            {
                return ReportGranularityEnum.Date;
            }
        }

        public static DateRange AltQuickDateToDateRange(QuickDateFilterAltEnum? typeEnum)
        {
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;
            var yesterday = today.AddDays(-1);
            var thisSunday = today.AddDays(-(int)today.DayOfWeek);
            var prevSunday = thisSunday.AddDays(-7);
            var prevPrevSunday = prevSunday.AddDays(-7);
            var thisMonthStart = today.AddDays(-today.Day + 1);
            var prevMonthStart = thisMonthStart.AddMonths(-1);
            var prevPrevMonthStart = thisMonthStart.AddMonths(-2);

            return typeEnum switch
            {
                QuickDateFilterAltEnum.LastWeek => new DateRange(prevPrevSunday, prevPrevSunday.AddDays(6)),
                QuickDateFilterAltEnum.LastMonth => new DateRange(prevPrevMonthStart.AddMonths(1), prevPrevMonthStart.AddMonths(2).AddDays(-1)),
                _ => new DateRange(yesterday, yesterday)
            };
        }

        // Generate period which are previous related to given value
        public static DateRange QuickDateToPrevDateRange(QuickDateFilterTypeEnum? typeEnum)
        {
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;
            var yesterday = today.AddDays(-1);
            var thisSunday = today.AddDays(-(int)today.DayOfWeek);
            var prevSunday = thisSunday.AddDays(-7);
            var prevPrevSunday = prevSunday.AddDays(-7);
            var thisMonthStart = today.AddDays(-today.Day + 1);
            var prevMonthStart = thisMonthStart.AddMonths(-1);
            var prevPrevMonthStart = thisMonthStart.AddMonths(-2);

            return typeEnum switch
            {
                QuickDateFilterTypeEnum.Today => new DateRange(yesterday, yesterday),
                QuickDateFilterTypeEnum.Yesterday => new DateRange(yesterday.AddDays(-1), yesterday.AddDays(-1)),
                QuickDateFilterTypeEnum.ThisWeek => new DateRange(prevSunday, prevSunday.AddDays(6)),
                QuickDateFilterTypeEnum.LastWeek => new DateRange(prevPrevSunday, prevPrevSunday.AddDays(6)),
                QuickDateFilterTypeEnum.Last30Days => new DateRange(today.AddDays(-59), today.AddDays(-30)),
                QuickDateFilterTypeEnum.ThisMonth => new DateRange(prevMonthStart, prevMonthStart.AddMonths(1).AddDays(-1)),
                QuickDateFilterTypeEnum.LastMonth => new DateRange(prevPrevMonthStart.AddMonths(1), prevPrevMonthStart.AddMonths(2).AddDays(-1)),
                QuickDateFilterTypeEnum.Last3Months => new DateRange(prevPrevMonthStart.AddMonths(-3), prevPrevMonthStart.AddDays(-1)),
                _ => new DateRange(today, today),
            };
        }
    }
}
