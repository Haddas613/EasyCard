using Newtonsoft.Json;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Models.Binding
{
    public class TrimmingDateTimeConverter : JsonConverter
    {
        public TrimmingDateTimeConverter()
        {
        }

        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => objectType == typeof(DateTime);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            DateTime? asDateTime = null;

            if (reader.TokenType == JsonToken.Date)
            {
                asDateTime = reader.Value as DateTime?;
            }

            if (reader.TokenType == JsonToken.String)
            {
                if (DateTime.TryParse((string)reader.Value, out var parsed))
                {
                    asDateTime = parsed;
                }
            }

            if (asDateTime.HasValue)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(asDateTime.Value, UserCultureInfo.TimeZone).Date;
            }

            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
