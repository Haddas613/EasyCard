using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers
{
    public static class StringHelper
    {
        public static string TrimAndNullIfWhiteSpace(this string text) =>
            string.IsNullOrWhiteSpace(text)
            ? null
            : text.Trim();

        public static string Left(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);

            return (value.Length <= maxLength
                   ? value
                   : value.Substring(0, maxLength)
                   );
        }

        public static string Right(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);

            return (value.Length <= maxLength
                   ? value
                   : value.Substring(value.Length - maxLength, maxLength)
                   );
        }

    }
}
