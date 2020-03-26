using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Common operation results information
    /// </summary>
    [DataContract]
    public partial class OperationResponse
    {
        public OperationResponse()
        {
            this.Errors = new List<Error>();
        }

        /// <summary>
        /// Gets or Sets EntityID
        /// </summary>
        [DataMember(Name = "entityID")]
        public long? EntityID { get; set; }

        /// <summary>
        /// Gets or Sets EntityReference
        /// </summary>
        [DataMember(Name = "entityReference")]
        public string EntityReference { get; set; }

        /// <summary>
        /// Gets or Sets Message
        /// </summary>
        [DataMember(Name = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Exception unique Id
        /// </summary>
        [DataMember(Name = "correlationId")]
        public string CorrelationId { get; set; }

        /// <summary>
        /// Operation Status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StatusEnum? Status { get; set; }

        /// <summary>
        /// Gets or Sets Errors
        /// </summary>
        [DataMember(Name = "errors")]
        public List<Error> Errors { get; set; }

        /// <summary>
        /// Concurrency Token
        /// </summary>
        [DataMember(Name = "concurrencyToken")]
        public string ConcurrencyToken { get; set; }
    }
}
