using Newtonsoft.Json;
using Shared.Api.Models.Binding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Merchants.Shared.Models.Attributes;

namespace Merchants.Api.Models.Merchant
{
    // TODO: use base class (see UpdateMerchantRequest)
    public class MerchantRequest
    {
        [JsonConverter(typeof(TrimmingJsonConverter))]
        [Required(AllowEmptyStrings = false)]
        [StringLength(50, MinimumLength = 3)]
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

        [StringLength(50)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string PhoneNumber { get; set; }
    }
}
