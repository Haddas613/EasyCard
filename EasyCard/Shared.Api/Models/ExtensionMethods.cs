using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace Shared.Api.Models
{
    public static class ExtensionMethods
    {
        public static bool IsIn(this int value, params int[] otherValues)
        {
            return otherValues.Contains(value);
        }
    }
}
