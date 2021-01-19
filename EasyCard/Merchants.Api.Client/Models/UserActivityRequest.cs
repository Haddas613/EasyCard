using Merchants.Shared.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Api.Client.Models
{
    public class UserActivityRequest
    {
        public string UserID { get; set; }

        public UserActivityEnum UserActivity { get; set; }

        public JObject AdditionalData { get; set; } 
    }
}
