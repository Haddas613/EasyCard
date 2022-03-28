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
    /// Additional deal information. All these data are not required and used only for merchant's business purposes.
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DealDetails
    {
        /// <summary>
        /// Deal identifier in merchant's system
        /// </summary>
        [StringLength(50)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string DealReference { get; set; }

        /// <summary>
        /// Deal description. In case of generating payment link, these description will be displayed on Checkout Page
        /// </summary>
        [StringLength(250)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string DealDescription { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        [StringLength(50)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [EmailAddress]
        public string ConsumerEmail { get; set; }

        /// <summary>
        /// End-customer Name
        /// </summary>
        [StringLength(50)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string ConsumerName { get; set; }

        /// <summary>
        /// End-customer National Id
        /// </summary>
        [StringLength(20)]
        public string ConsumerNationalID { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        [StringLength(20)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string ConsumerPhone { get; set; }

        /// <summary>
        /// End-customer record UUId in EasyCard system
        /// </summary>
        public Guid? ConsumerID { get; set; }

        /// <summary>
        /// Deal Items
        /// ID, Count, Name
        /// </summary>
        public IEnumerable<Item> Items { get; set; }

        /// <summary>
        /// End-customer address
        /// </summary>
        public Address ConsumerAddress { get; set; }

        /// <summary>
        /// External system consumer identifier for example RapidOne customer code
        /// </summary>
        [StringLength(50)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string ConsumerExternalReference { get; set; }

        [StringLength(50)]
        public string ConsumerWoocommerceID { get; set; }

        [StringLength(50)]
        public string ConsumerEcwidID { get; set; }
    }
}
