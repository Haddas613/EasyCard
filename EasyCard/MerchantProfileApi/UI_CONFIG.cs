using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MerchantProfileApi
{
    [DataContract]
    public class UI_CONFIG
    {
        [JsonProperty("VUE_APP_TRANSACTIONS_API_BASE_ADDRESS")]
        public string VUE_APP_TRANSACTIONS_API_BASE_ADDRESS { get; set; }

        [JsonProperty("VUE_APP_PROFILE_API_BASE_ADDRESS")]
        public string VUE_APP_PROFILE_API_BASE_ADDRESS { get; set; }

        [JsonProperty("VUE_APP_REPORT_API_BASE_ADDRESS")]
        public string VUE_APP_REPORT_API_BASE_ADDRESS { get; set; }

        [JsonProperty("VUE_APP_AUTHORITY")]
        public string VUE_APP_AUTHORITY { get; set; }

        [JsonProperty("VUE_APP_APPLICATION_INSIGHTS_KEY")]
        public string VUE_APP_APPLICATION_INSIGHTS_KEY { get; set; }

        [JsonProperty("VUE_APP_SUPPORT_EMAIL")]
        public string VUE_APP_SUPPORT_EMAIL { get; set; }
    }
}
