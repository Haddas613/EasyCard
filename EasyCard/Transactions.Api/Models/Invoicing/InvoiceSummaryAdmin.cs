using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Api.UI;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Invoicing
{
    public class InvoiceSummaryAdmin : InvoiceSummary
    {
        /// <summary>
        /// Currency
        /// </summary>
        [EnumDataType(typeof(CurrencyEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        [MetadataOptions(Hidden = true)]
        public new CurrencyEnum Currency { get; set; }
    }
}
