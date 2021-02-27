using Shared.Api.Models.Enums;
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

        // TODO: return only Date field
        public static DateTime QuickDateToDateTime(QuickDateFilterTypeEnum typeEnum)
            => typeEnum switch
            {
                // TODO: should be "Today"
                QuickDateFilterTypeEnum.Last24Hours => DateTime.UtcNow.AddHours(-24),
                QuickDateFilterTypeEnum.LastWeek => DateTime.UtcNow.AddDays(-7),
                QuickDateFilterTypeEnum.LastTwoWeeks => DateTime.UtcNow.AddDays(-14),
                QuickDateFilterTypeEnum.LastMonth => DateTime.UtcNow.AddMonths(-1),
                QuickDateFilterTypeEnum.LastYear => DateTime.UtcNow.AddYears(-1),
                _ => DateTime.MinValue,
            };
    }
}
