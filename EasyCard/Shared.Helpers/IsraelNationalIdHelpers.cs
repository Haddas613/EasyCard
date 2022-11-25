using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Shared.Helpers
{
    public static class IsraelNationalIdHelpers
    {
        public static bool Valid(string nationalID)
        {
            if (string.IsNullOrEmpty(nationalID))
            {
                return false;
            }

            nationalID = nationalID.Trim();
            if (string.IsNullOrWhiteSpace(nationalID) || nationalID.Length < 5 || nationalID.Length > 9 || !int.TryParse(nationalID, out var _))
            {
                return false;
            }

            try
            {
                if (nationalID.Length < 9)
                {
                    nationalID = nationalID.PadLeft(9, '0');
                }

                int i = 0;
                var sum = nationalID
                    .Select(s => int.Parse(s.ToString()))
                    .Aggregate((acc, val) =>
                    {
                        var step = val * ((++i % 2) + 1);
                        return acc + (step > 9 ? step - 9 : step);
                    });

                return sum % 10 == 0;
            }
            catch
            {
                return false;
            }
        }

        private static Regex digitsOnly = new Regex(@"[^\d]");

        public static string NormalizeIsraelNationalID(this string nationalIDStr)
        {
            if (string.IsNullOrWhiteSpace(nationalIDStr))
            {
                return null;
            }

            var res = digitsOnly.Replace(nationalIDStr, string.Empty);

            res = res.Left(9);

            return res.PadLeft(9, '0');
        }
    }
}
