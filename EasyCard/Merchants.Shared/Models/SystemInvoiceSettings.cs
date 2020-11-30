using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Merchants.Shared.Models
{
    public class SystemInvoiceSettings
    {
        [StringLength(250)]
        public string DefaultInvoiceSubject { get; set; }

        [EnumDataType(typeof(InvoiceTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public InvoiceTypeEnum? DefaultInvoiceType { get; set; }
    }
}
