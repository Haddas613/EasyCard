﻿using Newtonsoft.Json;
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
        public string DealReference { get; set; }

        /// <summary>
        /// Deal description
        /// </summary>
        [StringLength(250)]
        public string DealDescription { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        [StringLength(50)]
        public string ConsumerEmail { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        [StringLength(50)]
        public string ConsumerPhone { get; set; }

        /// <summary>
        /// End-customer reference
        /// </summary>
        public Guid? ConsumerID { get; set; }
    }
}
