﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Nayax.Models
{
    public class NayaxValidateRequest
    {
        public NayaxRequest TerminalDetails { get; set; }

        public string OwnerIdentityNumber { get; set; }

        [EnumDataType(typeof(EntryModeEnum))]
        public EntryModeEnum EntryMode { get; set; }

        public string Vuid { get; set; }

        [Required]
        [EnumDataType(typeof(TranTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public TranTypeEnum TranType { get; set; }

        /// <summary>
        /// Total Amount Authorized
        /// </summary>
        [Required]
        [Range(100, int.MaxValue)]
        public decimal TransactionAmount { get; set; }

        [Required(ErrorMessageResourceType = typeof(NayaxValidateRequestResource), ErrorMessageResourceName = "CardNumber")]
        [StringLength(64, MinimumLength = 10)]
        public string MaskedPan { get; set; }

        /// <summary>
        /// MMYY format ex:1216
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(NayaxValidateRequestResource), ErrorMessageResourceName = "CardExpiry")]
        [StringLength(4, MinimumLength = 4)]
        public string CardExpiry { get; set; }

        /// <summary>
        /// ILS = 376,
        /// DOLLAR= 840,
        /// EURO = 978
        /// </summary>
        [EnumDataType(typeof(CurrencyEnumISO_Code))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyEnumISO_Code OriginalCurrency { get; set; }

        public int PaymentsNumber { get; set; }

        /// <summary>
        /// In Agorot, with leading zeros
        /// </summary>
        public decimal FirstPaymentAmount { get; set; }

        /// <summary>
        /// In Agorot, with leading zeros
        /// </summary>
        public decimal NextPaymentAmount { get; set; }

        [Required]
        [EnumDataType(typeof(CreditTermsEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CreditTermsEnum CreditTerms { get; set; }
    }
}
