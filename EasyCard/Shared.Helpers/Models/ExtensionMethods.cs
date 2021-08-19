using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace Shared.Helpers.Models
{
    public static class ExtensionMethods
    {
        public static bool IsIn<T>(this T value, params T[] otherValues)
            where T : struct
        {
            return otherValues.Contains(value);
        }
    }
}
