using Common.Models.Enums;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace Common.Models
{
    public class TerminalInvoiceSettings
    {
        [StringLength(250)]
        public string DefaultInvoiceSubject { get; set; }

        [EnumDataType(typeof(InvoiceTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public InvoiceTypeEnum? DefaultInvoiceType { get; set; }

        [EnumDataType(typeof(InvoiceTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public InvoiceTypeEnum? DefaultRefundInvoiceType { get; set; }

        [EnumDataType(typeof(InvoiceTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public InvoiceTypeEnum? DefaultCreditInvoiceType { get; set; }

        // TODO: validation
        public string[] SendCCTo { get; set; }

        [StringLength(50)]
        public string EmailTemplateCode { get; set; }
    }
}
