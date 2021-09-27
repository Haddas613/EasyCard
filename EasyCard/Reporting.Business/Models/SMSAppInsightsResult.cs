using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reporting.Business.Models
{
    public class SMSAppInsightsResult
    {
        [JsonProperty("timestamp")]
        public DateTime? Timestamp { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }
    }
}
