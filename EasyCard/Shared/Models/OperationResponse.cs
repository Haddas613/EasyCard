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
        public OperationResponse() { }
        public OperationResponse(string message, StatusEnum status, long? entityId = null) 
        {
            Message = message;
            Status = status;
            EntityID = entityId;
        }

        public string Message { get; set; }

        [EnumDataType(typeof(StatusEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public StatusEnum? Status { get; set; }

        public long? EntityID { get; set; }

        public string EntityReference { get; set; }
    }
}
