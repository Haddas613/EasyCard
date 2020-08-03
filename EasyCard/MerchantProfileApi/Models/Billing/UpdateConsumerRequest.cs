using Newtonsoft.Json;
using Shared.Api.Models.Binding;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Billing
{
    /// <summary>
    /// Update end-customer info
    /// </summary>
    public class UpdateConsumerRequest : IConcurrencyCheck
    {
        /// <summary>
        ///  Target record ID
        /// </summary>
        [Required]
        public Guid ConsumerID { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        [JsonConverter(typeof(TrimmingJsonConverter))]
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
        [StringLength(50)]
        public string ConsumerPhone { get; set; }

        /// <summary>
        /// End-customer National ID
        /// </summary>
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [StringLength(50)]
        public string ConsumerNationalID { get; set; }

        /// <summary>
        /// End-customer address
        /// </summary>
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [StringLength(250)]
        public string ConsumerAddress { get; set; }

        /// <summary>
        /// Concurrency check
        /// </summary>
        [Required]
        public byte[] UpdateTimestamp { get; set; }
    }
}
