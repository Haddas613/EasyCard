using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Terminal
{
    public class TerminalInvoiceSettingsUpdate
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

        public bool PaymentInfoAsDonation { get; set; }
    }
}
