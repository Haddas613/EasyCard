using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServerClient
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ApiKeyOperationResponse
    {
        public ApiKeyOperationResponse()
        {
            Status = Shared.Api.Models.Enums.StatusEnum.Success;
        }

        public string ApiKey { get; set; }

        public string Message { get; set; }

        public string WoocommerceApiKey { get; set; }

        public string EcwidApiKey { get; set; }

        public Shared.Api.Models.Enums.StatusEnum Status { get; set; }
    }
}
