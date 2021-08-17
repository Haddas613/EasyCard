using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shared.Helpers
{
    public static class IsraelNationalIdHelpers
    {
        public static bool Valid(string nationalID)
        {
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
    }
}
