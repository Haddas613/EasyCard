using Merchants.Shared.Enums;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Terminal
{
    public class TerminalSummary
    {
        public Guid TerminalID { get; set; }

        public string Label { get; set; }

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
