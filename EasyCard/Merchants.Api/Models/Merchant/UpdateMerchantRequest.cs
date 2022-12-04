using Merchants.Shared.Models.Attributes;
using Newtonsoft.Json;
using Shared.Api.Models.Binding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Merchant
{
    // TODO: use base class (see MerchantRequest)
    public class UpdateMerchantRequest
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(50, MinimumLength = 3)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [NoSpecialChars]
        public string BusinessName { get; set; }

        [StringLength(50, MinimumLength = 3)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [NoSpecialChars]
        public string MarketingName { get; set; }

        [StringLength(9, MinimumLength = 9)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string BusinessID { get; set; }

        [StringLength(50, MinimumLength = 3)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string ContactPerson { get; set; }
    }
}
