using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared.Models
{
    public class OperationResponse
    {
        public string Message { get; set; }

        [EnumDataType(typeof(StatusEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public StatusEnum? Status { get; set; }

        public long? EntityID { get; set; }
    }
}
