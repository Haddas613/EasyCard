using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServerClient
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserOperationResponse
    {
        public Guid UserID { get; set; }

        public UserOperationResponseCodeEnum ResponseCode { get; set; }

        public string Message { get; set; }
    }
}
