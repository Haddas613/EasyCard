using Newtonsoft.Json;
using Shared.Helpers.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
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

        public static bool HasExcelIgnoreAttribute(this PropertyInfo pr)
        {
            var attribute = pr.GetCustomAttribute(typeof(ExcelIgnoreAttribute), false);

            return attribute != null;
        }

        public static Dictionary<string, string> GetExcelColumnNames<T>(this ResourceManager translations, string[] columnsOrder = null, bool showOnlyGivenInColumnsOrder = false)
        {
            var columnNames = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => !p.HasExcelIgnoreAttribute())
                .Select(p => p.Name).ToList();

            var mapping = new Dictionary<string, string>();

            if (columnsOrder != null)
            {
                foreach (var columnName in columnsOrder)
                {
                    if (!columnNames.Contains(columnName))
                    {
                        continue;
                    }

                    var columnHeader = translations?.GetString(columnName) ?? columnName;
                    mapping.Add(columnName, columnHeader);
                }

                if (!showOnlyGivenInColumnsOrder)
                {
                    foreach (var columnName in columnNames.Where(d => !columnsOrder.Contains(d)))
                    {
                        var columnHeader = translations?.GetString(columnName) ?? columnName;
                        mapping.Add(columnName, columnHeader);
                    }
                }
            }
            else
            {
                foreach (var columnName in columnNames)
                {
                    var columnHeader = translations?.GetString(columnName) ?? columnName;
                    mapping.Add(columnName, columnHeader);
                }
            }

            return mapping;
        }

        public static T Clone<T>(T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
