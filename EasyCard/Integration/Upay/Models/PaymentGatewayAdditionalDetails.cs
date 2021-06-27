﻿using System.Runtime.Serialization;

namespace Upay.Models
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
        ///  Shva Auth number
        /// </summary>
        [DataMember(Name = "shvaAuthNum")]
        public string ShvaAuthNum { get; set; }
    }
}