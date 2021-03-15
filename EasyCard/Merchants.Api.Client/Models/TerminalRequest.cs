using Newtonsoft.Json;
using Shared.Api.Models.Binding;
using System;
using System.ComponentModel.DataAnnotations;

namespace Merchants.Api.Client.Models
{
    public class TerminalRequest
    {
        [Required]
        public Guid MerchantID { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(50, MinimumLength = 6)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string Label { get; set; }

        public long? TerminalTemplateID { get; set; }
    }
}
