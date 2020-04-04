using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers
{
    public class UserCultureInfo
    {
        /// <summary>
        /// Initializes static members of the <see cref="UserCultureInfo"/> class.
        /// </summary>
        static UserCultureInfo()
        {
            // TODO: Need to through config or user claims
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
            DateTimeFormat = "dd/MM/yyyy hh:mm:ss"; // Default format.
            DateFormat = "dd/MM/yyyy"; // Default format.
        }

        public static string DateFormat { get;  }

        public static string DateTimeFormat { get;  }

        public static TimeZoneInfo TimeZone { get; set; }

        public static DateTime GetUserLocalTime(DateTime utcTime)
        {
            return TimeZoneInfo.ConvertTime(utcTime, TimeZone);
        }

        public static DateTime GetUtcTime(DateTime datetime)
        {
            return TimeZoneInfo.ConvertTime(datetime, TimeZone).ToUniversalTime();
        }
    }
}
