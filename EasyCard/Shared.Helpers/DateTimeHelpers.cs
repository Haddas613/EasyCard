using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers
{
    public static class DateTimeHelpers
    {
        public static DateTime LastDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
        }
    }
}
