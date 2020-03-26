using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Shared.Api.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class OperationResponse
    {
        public OperationResponse()
        {
        }

        public OperationResponse(string message, StatusEnum status)
        {
            Message = message;
            Status = status;
        }

        public OperationResponse(string message, StatusEnum status, long? entityId = null, string correlationId = null, IEnumerable<Api.Models.Error> errors = null)
        {
            Message = message;
            Status = status;
            EntityID = entityId;
            CorrelationId = correlationId;
            Errors = errors?.Select(d => d).ToList();
        }

        public OperationResponse(string message, StatusEnum status, string entityReference = null)
        {
            Message = message;
            Status = status;
            EntityReference = entityReference;
        }

        public string Message { get; set; }

        [EnumDataType(typeof(StatusEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public StatusEnum? Status { get; set; }

        public long? EntityID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EntityReference { get; set; }

        public string CorrelationId { get; internal set; }

        public string EntityType { get; set; }

        public IEnumerable<Error> Errors { get; set; }

        public string ConcurrencyToken { get; set; }
    }
}
