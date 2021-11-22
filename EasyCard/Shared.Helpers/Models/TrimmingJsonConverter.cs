using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Models.Binding
{
    public class TrimmingJsonConverter : JsonConverter
    {
        private readonly bool nullIfEmpty = true;

        public TrimmingJsonConverter()
        {
        }

        public TrimmingJsonConverter(bool nullIfEmpty)
        {
            this.nullIfEmpty = nullIfEmpty;
        }

        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => objectType == typeof(string);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var res = ((string)reader.Value)?.Trim();

                if (nullIfEmpty)
                {
                    return res == string.Empty ? null : res;
                }
                else
                {
                    return res;
                }
            }

            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
