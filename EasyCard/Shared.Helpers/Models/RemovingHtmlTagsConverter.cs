using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Shared.Api.Models.Binding
{
    public class RemovingHtmlTagsConverter : JsonConverter
    {
        public RemovingHtmlTagsConverter()
        {
        }

        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => objectType == typeof(string);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var res = ((string)reader.Value)?.Trim();

                if (res != null)
                {
                    return Regex.Replace(res, @"<(.|\n)*?>", string.Empty);
                }

                return res;
            }

            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
