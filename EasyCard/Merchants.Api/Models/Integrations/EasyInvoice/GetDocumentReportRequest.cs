using Shared.Api.Models.Binding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Integrations.EasyInvoice
{
    public class GetDocumentReportRequest
    {
        [Required]
        public Guid TerminalID { get; set; }

        [Required]
        [JsonConverter(typeof(TrimmingDateTimeConverter))]
        public DateTime StartAt { get; set; }//yyyy-mm-dd

        [Required]
        [JsonConverter(typeof(TrimmingDateTimeConverter))]
        public DateTime EndAt { get; set; }

        public bool OnlyCancelled { get; set; }
        public bool IncludeCancelled { get; set; }
    }
}
