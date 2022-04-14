using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Shared.Api.Models.Binding
{
    public class TrimmingJsonConverter : JsonConverter
    {
        private static readonly Regex SWhitespace = new Regex(@"\s+");

        private readonly bool nullIfEmpty = true;
        private readonly bool removeAllSpaces = false;

        public TrimmingJsonConverter()
        {
        }

        public TrimmingJsonConverter(bool nullIfEmpty)
        {
            this.nullIfEmpty = nullIfEmpty;
        }

        public TrimmingJsonConverter(bool nullIfEmpty, bool removeAllSpaces)
        {
            this.nullIfEmpty = nullIfEmpty;
            this.removeAllSpaces = removeAllSpaces;
        }

        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => objectType == typeof(string);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var res = ((string)reader.Value)?.Trim();

                if (removeAllSpaces)
                {
                    res = SWhitespace.Replace(res, string.Empty);
                }

                if (nullIfEmpty)
                {
                    res = res == string.Empty ? null : res;
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
