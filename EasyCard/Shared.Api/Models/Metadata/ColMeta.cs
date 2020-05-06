using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Shared.Api.Models.Metadata
{
    public class ColMeta
    {
        [JsonIgnore]
        public string Key { get; set; }

        public string Name { get; set; }

        public string DataType { get; set; }
    }
}
