using Shared.Api.Models.Metadata;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Shared.Api.UI
{
    public static class MetadataHelpers
    {
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

            return new ColMeta
            {
                Key = property.Name.ToCamelCase(), // TODO: use data contract attribute
                Name = description ?? property.Name,
                DataType = dataType,
                Dictionary = dictionary
            };
        }
    }
}
