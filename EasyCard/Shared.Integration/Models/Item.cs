﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Api.Models.Binding;
using Shared.Api.Swagger;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Integration.Models
{
    public class Item
    {
        public Guid? ItemID { get; set; }

        [StringLength(250)]
        public string ExternalReference { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(250)]
        [JsonConverter(typeof(RemovingHtmlTagsConverter))]
        public string ItemName { get; set; }

        [StringLength(50)]
        public string SKU { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? NetPrice { get; set; }

        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? Quantity { get; set; }

        /// <summary>
        /// Row amount
        /// </summary>
        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? Amount { get; set; }

        /// <summary>
        /// VAT Rate
        /// </summary>
        [Range(0, 1)]
        [DataType(DataType.Currency)]
        public decimal? VATRate { get; set; }

        /// <summary>
        /// VAT amount
        /// </summary>
        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? VAT { get; set; }

        /// <summary>
        /// Net amount (before VAT)
        /// </summary>
        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// Discount
        /// </summary>
        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? Discount { get; set; }

        /// <summary>
        /// Discount
        /// </summary>
        [Range(0, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal? NetDiscount { get; set; }

        [SwaggerExclude]
        public JObject Extension { get; set; }

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
