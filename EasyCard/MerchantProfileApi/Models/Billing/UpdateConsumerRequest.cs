﻿using Newtonsoft.Json;
using Shared.Api.Models.Binding;
using Shared.Api.Swagger;
using Shared.Helpers;
using Shared.Helpers.Models;
using Shared.Integration.Models;
using Shared.Integration.Models.PaymentDetails;
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
        [EmailAddress]
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
        [Required(AllowEmptyStrings = false)]
        [IsraelNationalIDValidator]
        public string ConsumerNationalID { get; set; }

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
        /// Origin of customer
        /// </summary>
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [StringLength(50)]
        public string Origin { get; set; }

        /// <summary>
        /// Concurrency check
        /// </summary>
        [Required]
        public byte[] UpdateTimestamp { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [SwaggerExclude]
        [StringLength(50)]
        public string ConsumerSecondPhone { get; set; }

        /// <summary>
        /// ID in BillingDesktop system
        /// </summary>
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [StringLength(50)]
        [SwaggerExclude]
        public string BillingDesktopRefNumber { get; set; }

        // TODO: remove this property - add method for customer reqctivation
        public bool Active { get; set; } = true;

        public BankDetails BankDetails { get; set; }

        /// <summary>
        /// External ID inside https://woocommerce.com system
        /// </summary>
        [StringLength(50)]
        public string WoocommerceID { get; set; }

        /// <summary>
        /// External ID inside https://www.ecwid.com system
        /// </summary>
        [StringLength(50)]
        public string EcwidID { get; set; }
    }
}
