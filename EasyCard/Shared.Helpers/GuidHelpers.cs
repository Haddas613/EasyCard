using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers
{
    public static class GuidHelpers
    {
        /// <summary>
        /// Base 64 guid representation
        /// </summary>
        public static string GetStringReference(this Guid guid)
        {
            return System.Convert.ToBase64String(guid.ToByteArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        /// <summary>
        /// Get original guid based on string reference
        /// </summary>
        public static Guid RestoreFromStringReference(this string guidReference)
        {
            string incoming = guidReference
                .Replace('_', '/').Replace('-', '+');
            switch (guidReference.Length % 4)
            {
                case 2: incoming += "=="; break;
                case 3: incoming += "="; break;
            }

            return new Guid(Convert.FromBase64String(incoming));
        }

        /// <summary>
        /// Generate guid which is sortable in SQL server
        /// </summary>
        public static Guid GetSequentialGuid(this Guid guid, DateTime dateTime)
        {
            byte[] guidArray = guid.ToByteArray();

            DateTime baseDate = new DateTime(1900, 1, 1);
            DateTime now = dateTime; // DateTime.Now;

            // Get the days and milliseconds which will be used to build the byte string
            TimeSpan days = new TimeSpan(now.Ticks - baseDate.Ticks);
            TimeSpan msecs = now.TimeOfDay;

            // Convert to a byte array
            // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333
            byte[] daysArray = BitConverter.GetBytes(days.Days);
            byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

            // Reverse the bytes to match SQL Servers ordering
            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);

            // Copy the bytes into the guid
            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

            return new Guid(guidArray);
        }

        public static string GetSortableStr(this Guid guid, DateTime dateTime)
        {
            return $"{dateTime.ToString("yyyyMMdd-HHmmss-fff")}-{guid.ToString().Replace("-", string.Empty).Substring(0, 16)}";
        }
    }
}
