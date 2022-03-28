using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Api.Models.Binding;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Transactions.Business.Entities
{
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
        [StringLength(500)]
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

        [StringLength(20)]
        public string ConsumerNationalID { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        [StringLength(20)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string ConsumerPhone { get; set; }

        /// <summary>
        /// Consumer ID
        /// </summary>
        public Guid? ConsumerID { get; set; }

        /// <summary>
        /// Deal Items
        /// </summary>
        public IEnumerable<Item> Items { get; set; }

        /// <summary>
        /// End-customer Address
        /// </summary>
        public Address ConsumerAddress { get; set; }

        /// <summary>
        /// External system consumer code for example Rapid customer code
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
