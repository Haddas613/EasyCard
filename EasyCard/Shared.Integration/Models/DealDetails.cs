using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Api.Models.Binding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [StringLength(50)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string DealReference { get; set; }

        /// <summary>
        /// Deal description
        /// </summary>
        [StringLength(250)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string DealDescription { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        [StringLength(50)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string ConsumerEmail { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        [StringLength(50)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string ConsumerPhone { get; set; }

        /// <summary>
        /// End-customer reference
        /// </summary>
        public Guid? ConsumerID { get; set; }

        /// <summary>
        /// Deal Items
        /// ID, Count, Name
        /// </summary>
        public JObject Items { get; set; }
    }
}
