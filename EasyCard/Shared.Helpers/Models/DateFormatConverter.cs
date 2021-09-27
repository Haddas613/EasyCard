using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Shared.Api.Models
{
    public class DateFormatConverter : DateTimeConverterBase
    {
        private readonly DateFormatType dateFormatType;
        private readonly string dateFormat;

        public DateFormatConverter(DateFormatType dateFormatType)
        {
            this.dateFormatType = dateFormatType;
        }

        public DateFormatConverter(string dateFormat)
        {
            this.dateFormat = dateFormat;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Null)
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    JValue jValue = new JValue(reader.Value);

                    switch (reader.TokenType)
                    {
                        case JsonToken.String:
                            if (string.IsNullOrWhiteSpace((string)jValue))
                            {
                                return null;
                            }

                            return jValue.ToObject<DateTime?>();
                        case JsonToken.Date:
                            return (DateTime)jValue;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer
                .WriteValue(Convert.ToDateTime(value).ToString(dateFormat ?? (dateFormatType == DateFormatType.DateTime ? "o" : "yyyy'-'MM'-'dd")));

            writer.Flush();
        }
    }
}
