using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared.Helpers
{
    public static class ReflectionHelpers
    {
        public static string GetDataContractAttrForEnum(this Type enumType, string enumMember)
        {
            var memInfo = enumType.GetMember(enumMember);
            var attributes = memInfo[0].GetCustomAttributes(typeof(EnumMemberAttribute), false);
            if (attributes.Length > 0)
            {
                var description = ((EnumMemberAttribute)attributes[0]).Value;
                return description;
            }
            else
            {
                return enumMember;
            }
        }
    }
}
