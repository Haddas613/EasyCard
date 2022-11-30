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
            reader.Read();
            if (reader.GetType() == typeof(JTokenReader))
           {
                var res = ((string)reader.Value)?.Trim() == string.Empty ? null : ((string)reader.Value)?.Trim();
                if (res != null)
                {
                    res = Regex.Replace(res, @"<script>(.|\n)*?</script>", string.Empty); // remove script tags and its content
                    res = Regex.Replace(res, @"<style>(.|\n)*?</style>", string.Empty); // remove style tags and its content
                    res = Regex.Replace(res, @"<xml>(.|\n)*?</xml>", string.Empty); //  remove xml tags and its content
                    res = Regex.Replace(res, @"<(.|\n)*?>", string.Empty); // remove html tags but not text content inside
                    res = Regex.Replace(res, @" {2,}", " ").Trim(); // remove multiple spaces, remains after removing html tags or existing in text and trim
                    return res;
                }

                return null;
           }

            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
