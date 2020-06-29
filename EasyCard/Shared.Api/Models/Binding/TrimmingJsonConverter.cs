using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Models.Binding
{
    public class TrimmingJsonConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => objectType == typeof(string);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                return ((string)reader.Value)?.Trim();
            }

            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
