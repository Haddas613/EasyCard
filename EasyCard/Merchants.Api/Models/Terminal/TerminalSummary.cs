using Merchants.Api.Models.Merchant;
using Merchants.Shared.Enums;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Terminal
{
    public class TerminalSummary
    {
        public long TerminalID { get; set; }

        public string Label { get; set; }

        public MerchantSummary Merchant { get; set; }

        /// <summary>
        /// Shva or other processor
        /// </summary>
        public ExternalSystemSummary Processor { get; set; }

        /// <summary>
        /// Clearing House or Upay
        /// </summary>
        public ExternalSystemSummary Aggregator { get; set; }

        /// <summary>
        /// EasyInvoice or RapidOne
        /// </summary>
        public ExternalSystemSummary Invoicing { get; set; }

        /// <summary>
        /// TODO: change ExternalSystemSummary to (?)
        /// </summary>
        public ExternalSystemSummary Marketer { get; set; }

        /// <summary>
        /// SHVA or other system terminal ID
        /// </summary>
        public string ExternalProcessorReference { get; set; }

        [EnumDataType(typeof(TerminalStatusEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public TerminalStatusEnum Status { get; set; }

        public DateTime? ActivityStartDate { get; set; }
    }
}
