using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Models.Metadata
{
    public class ColMeta
    {
        [JsonIgnore]
        public string Key { get; set; }

        public string Name { get; set; }

        public string DataType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Dictionary { get; set; }
    }
}
