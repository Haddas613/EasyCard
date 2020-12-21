using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.User
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserSummary
    {
        public Guid UserID { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }
}
