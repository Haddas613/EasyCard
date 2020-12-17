using Merchants.Shared.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Terminal
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ExternalSystemSummary
    {
        public long ExternalSystemID { get; set; }

        public ExternalSystemTypeEnum Type { get; set; }

        public string Name { get; set; }

        public string Key { get; set; }
    }
}
