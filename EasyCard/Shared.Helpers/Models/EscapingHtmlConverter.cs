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

        public override bool CanRead => false;

        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType) => objectType == typeof(string);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var str = value as string;
            if (str != null)
            {
                var res = System.Web.HttpUtility.HtmlEncode(value);
                writer.WriteValue(res);
            }
        }
    }
}
