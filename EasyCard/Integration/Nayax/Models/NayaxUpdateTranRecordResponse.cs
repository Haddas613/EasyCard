using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Nayax.Models
{
    public class NayaxUpdateTranRecordResponse
    {
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("error_msg")]
        public string ErrorMsg { get; set; }

        [JsonProperty("correlationID")]
        public string CorrelationID { get; set; }

        [JsonProperty("receipt_number")]
        public string ReceiptNumber { get; set; }
    }
}
