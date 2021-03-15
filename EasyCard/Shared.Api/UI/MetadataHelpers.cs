using Shared.Api.Models.Metadata;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Shared.Api.UI
{
    public static class MetadataHelpers
    {
        public static Dictionary<string, ColMeta> GetObjectMeta(this Type type, ResourceManager resourceManager, CultureInfo culture) => type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.GetColMeta(resourceManager, culture))
            .Where(p => !p.Hidden)
            .OrderBy(p => p.Order)
            .ToDictionary(d => d.Key);

        public static ColMeta GetColMeta(this PropertyInfo property, ResourceManager resourceManager, CultureInfo culture)
        {
            var description = resourceManager.GetString(property.Name, culture);
            string dictionary = null;
            string dataType = "string";

            if (property.PropertyType.IsEnum)
            {
                dictionary = property.PropertyType.Name.ToCamelCase();
                dataType = "dictionary";
            }
            else
            {
                if (property.PropertyType.IsAssignableFrom(typeof(decimal?)) || property.PropertyType.IsAssignableFrom(typeof(decimal)))
                {
                    dataType = "money"; // TODO: other decimal cases
                }
                else if (property.PropertyType.IsAssignableFrom(typeof(DateTime?)) || property.PropertyType.IsAssignableFrom(typeof(DateTime)))
                {
                    dataType = "date"; // TODO: datetime or date attribute
                }
                else if (property.PropertyType.IsAssignableFrom(typeof(Guid?)) || property.PropertyType.IsAssignableFrom(typeof(Guid)))
                {
                    dataType = "guid";
                }
            }

            var attributes = property.GetCustomAttributes(typeof(MetadataOptionsAttribute), false);
            MetadataOptionsAttribute metaAttribute = null;

            if (attributes.Length > 0)
            {
                metaAttribute = (MetadataOptionsAttribute)attributes[0];
            }

            return new ColMeta
            {
                Key = property.Name.ToCamelCase(), // TODO: use data contract attribute
                Name = description ?? property.Name,
                DataType = dataType,
                Dictionary = dictionary,
                Hidden = metaAttribute?.Hidden == true,
                Order = (metaAttribute?.Order).GetValueOrDefault(1000)
            };
        }
    }
}
