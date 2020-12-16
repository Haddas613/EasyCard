using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Api.UI;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions.Enums;
using Transactions.Shared.Enums;

namespace Transactions.Api.Models.Transactions
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TransactionSummaryAdmin : TransactionSummary
    {
        public string MerchantName { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid MerchantID { get; set; }
    }
}
