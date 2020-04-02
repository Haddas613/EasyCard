using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    /// <summary>
    /// Additional deal information
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DealDetails
    {
        /// <summary>
        /// Deal reference on merchant side
        /// </summary>
        public string DealReference { get; set; }

        /// <summary>
        /// Deal description
        /// </summary>
        public string DealDescription { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        public string ConsumerEmail { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        public string ConsumerPhone { get; set; }
    }
}
