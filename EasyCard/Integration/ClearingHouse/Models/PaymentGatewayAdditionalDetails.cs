using Newtonsoft.Json;
using Shared.Integration.Models;
using System.Runtime.Serialization;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Additional transaction details in Payment Gateway system
    /// </summary>
    [DataContract]
    public partial class PaymentGatewayAdditionalDetails
    {
        /// <summary>
        /// Shva Shovar Number
        /// </summary>
        [DataMember(Name = "shvaShovarNumber")]
        public string ShvaShovarNumber { get; set; }

        /// <summary>
        /// Shva deal ID
        /// </summary>
        [DataMember(Name = "shvaShovarData")]
        public string ShvaShovarData { get; set; }

        /// <summary>
        ///  Shva Transmission Number
        /// </summary>
        [DataMember(Name = "shvaTransmissionNumber")]
        public string ShvaTransmissionNumber { get; set; }

        /// <summary>
        ///  solek
        /// </summary>
        [JsonIgnore]
        public SolekEnum Solek { get; set; }
    }
}
