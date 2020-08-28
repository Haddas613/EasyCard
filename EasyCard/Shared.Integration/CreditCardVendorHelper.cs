using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shared.Integration
{
    public static class CreditCardVendorHelper
    {
        public static CardVendorEnum GetCardVendor(this string cardDigits)
        {
            if (string.IsNullOrWhiteSpace(cardDigits))
            {
                return CardVendorEnum.UNKNOWN;
            }

            if (cardDigits.StartsWith("4", StringComparison.InvariantCultureIgnoreCase))
            {
                return CardVendorEnum.VISA;
            }

            if (cardDigits.StartsWithRange(Enumerable.Range(51, 55), Enumerable.Range(2221, 2720)))
            {
                return CardVendorEnum.MASTERCARD;
            }

            if (cardDigits.StartsWithRange(new int[] { 34, 37 }))
            {
                return CardVendorEnum.AMEX;
            }

            if (cardDigits.StartsWithRange(new int[] { 36, 54 }, Enumerable.Range(300, 305)))
            {
                return CardVendorEnum.DINERS_CLUB;
            }

            return CardVendorEnum.OTHER;
        }

        public static bool StartsWithRange(this string str, IEnumerable<int> range1, IEnumerable<int> range2 = null, IEnumerable<int> range3 = null)
        {
            if (str.StartsWithRange(range1?.Select(d => d.ToString())))
            {
                return true;
            }

            if (str.StartsWithRange(range2?.Select(d => d.ToString())))
            {
                return true;
            }

            if (str.StartsWithRange(range3?.Select(d => d.ToString())))
            {
                return true;
            }

            return false;
        }

        public static bool StartsWithRange(this string str, IEnumerable<string> range)
        {
            if (range != null)
            {
                foreach (var r in range)
                {
                    if (str.StartsWith(r, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
