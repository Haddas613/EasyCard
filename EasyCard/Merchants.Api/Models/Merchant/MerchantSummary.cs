using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Merchant
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class MerchantSummary : MerchantBase
    {
    }
}
