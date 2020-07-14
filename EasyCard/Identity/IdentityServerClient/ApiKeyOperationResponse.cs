using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServerClient
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ApiKeyOperationResponse
    {
        public string ApiKey { get; set; }

        public string Message { get; set; }
    }
}
