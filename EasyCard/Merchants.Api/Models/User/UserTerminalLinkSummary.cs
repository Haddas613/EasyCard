using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.User
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserTerminalLinkSummary
    {
        public Guid TerminalID { get; set; }

        public string Label { get; set; }

        public string MerchantBusinessName { get; set; }

        public Guid? MerchantID { get; set; }

        public DateTime? Linked { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }
}
