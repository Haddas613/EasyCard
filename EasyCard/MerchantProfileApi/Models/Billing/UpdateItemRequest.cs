﻿using Newtonsoft.Json;
using Shared.Api.Models.Binding;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Billing
{
    public class UpdateItemRequest
    {
        public Guid ItemID { get; set; }

        [JsonConverter(typeof(TrimmingJsonConverter))]
        [Required(AllowEmptyStrings = false)]
        [StringLength(128, MinimumLength = 3)]
        public string ItemName { get; set; }

        public decimal Price { get; set; }

        public CurrencyEnum Currency { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        [StringLength(50)]
        public string ExternalReference { get; set; }

        [StringLength(50)]
        public string SKU { get; set; }
    }
}
