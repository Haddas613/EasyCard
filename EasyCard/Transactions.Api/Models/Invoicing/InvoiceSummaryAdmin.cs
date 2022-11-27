using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Api.UI;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Shared.Enums;

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

        public string MerchantName { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid MerchantID { get; set; }

        public string TerminalName { get; set; }

        [MetadataOptions(Hidden = true)]
        public new Guid? TerminalID { get; set; }

        [MetadataOptions(Hidden = true)]
        public new DateTime? InvoiceTimestamp { get; set; }

        public DocumentOriginEnum DocumentOrigin { get; set; }
    }
}
