using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            maxLength = Math.Abs(maxLength);

            return value.Length <= maxLength
                   ? value
                   : value.Substring(0, maxLength)
                   ;
        }

        public static string Right(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            maxLength = Math.Abs(maxLength);

            return value.Length <= maxLength
                   ? value
                   : value.Substring(value.Length - maxLength, maxLength)
                   ;
        }

        /// <summary>
        /// Will replace all '*' symbols in string with '%'. Depending on param may also enclose string with '%'
        /// </summary>
        /// <param name="src"></param>
        /// <param name="encloseInPercents">If true string will be enclosed with '%'. E.g. "%some text%" </param>
        /// <returns></returns>
        public static string UseWildCard(this string src, bool encloseInPercents) => encloseInPercents ? $"%{src.Replace("*", "%").Trim()}%" : $"{src.Replace("*", "%").Trim()}";

        public static string ToCamelCase(this string value)
        {
            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        public static string ComputeSha256Hash(this string rawData)
        {
            using (System.Security.Cryptography.SHA256 sha256Hash = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                return Convert.ToBase64String(bytes);
            }
        }

        public static string ConvertFromBase64(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            switch (str.Length % 4)
            {
                case 2: str += "=="; break;
                case 3: str += "="; break;
            }

            try
            {
                var res = Convert.FromBase64String(str);

                return Encoding.UTF8.GetString(res);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
