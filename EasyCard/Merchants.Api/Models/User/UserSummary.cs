using Merchants.Shared.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Api.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.User
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserSummary
    {
        public Guid UserID { get; set; }

        public Guid MerchantID { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        [EnumDataType(typeof(UserStatusEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public UserStatusEnum Status { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }
}
