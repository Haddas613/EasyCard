using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Models.Binding
{
    public class EscapingHtmlConverter : JsonConverter
    {
        public EscapingHtmlConverter()
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

                return res == null ? null : JsonConvert.SerializeObject(
                                            res,
                                            Formatting.None,
                                            new JsonSerializerSettings
                                            { StringEscapeHandling = StringEscapeHandling.EscapeHtml });
           }

            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
