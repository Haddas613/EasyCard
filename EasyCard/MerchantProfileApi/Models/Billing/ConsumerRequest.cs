using Newtonsoft.Json;
using Shared.Api.Models.Binding;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Billing
{
    /// <summary>
    /// Create consumer
    /// </summary>
    public class ConsumerRequest
    {
        /// <summary>
        /// Target terminal
        /// </summary>
        public Guid? TerminalID { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [Required(AllowEmptyStrings = false)]
        [StringLength(50)]
        public string ConsumerEmail { get; set; }

        /// <summary>
        /// End-customer name
        /// </summary>
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [Required(AllowEmptyStrings = false)]
        [StringLength(50, MinimumLength = 3)]
        public string ConsumerName { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [Required(AllowEmptyStrings = false)]
        [StringLength(50)]
        public string ConsumerPhone { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [Required(AllowEmptyStrings = false)]
        [StringLength(50)]
        public string ConsumerPhone2 { get; set; }

        /// <summary>
        /// End-customer National ID
        /// </summary>
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [StringLength(50)]
        public string ConsumerNationalID { get; set; }

        /// <summary>
        /// End-customer note details
        /// </summary>
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [StringLength(256)]
        public string Note { get; set; }

        /// <summary>
        /// End-customer address
        /// </summary>
        public Address ConsumerAddress { get; set; }

        /// <summary>
        /// ID in external system
        /// </summary>
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [StringLength(50)]
        public string ExternalReference { get; set; }

        /// <summary>
        /// ID in BillingDesktop system
        /// </summary>
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [StringLength(50)]

        public string BillingDesktopRefNumber { get; set; }

        /// <summary>
        /// Origin of customer
        /// </summary>
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [StringLength(50)]
        public string Origin { get; set; }
    }
}
