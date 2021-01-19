using Merchants.Shared.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.User
{
    public class UserActivityRequest
    {
        public string UserID { get; set; }

        public UserActivityEnum UserActivity { get; set; }

        public JObject AdditionalData { get; set; }
    }
}
