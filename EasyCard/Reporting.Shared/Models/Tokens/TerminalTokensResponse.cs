using Merchants.Shared.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Api.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Reporting.Shared.Models.Tokens
{
    public class TerminalTokensResponse
    {
        [MetadataOptions(Hidden = true)]
        public Guid TerminalID { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid? MerchantID { get; set; }

        public string Label { get; set; }

        /// <summary>
        /// SHVA or other system terminal ID
        /// </summary>
        public string AggregatorTerminalReference { get; set; }

        /// <summary>
        /// SHVA or other system terminal ID
        /// </summary>
        public string ProcessorTerminalReference { get; set; }

        [EnumDataType(typeof(TerminalStatusEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public TerminalStatusEnum Status { get; set; }

        public DateTime? ActivityStartDate { get; set; }

        public int Total { get; set; }

        public int Expired { get; set; }

        public int Deleted { get; set; }
    }
}
