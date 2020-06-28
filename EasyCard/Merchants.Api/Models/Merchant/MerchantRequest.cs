using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Merchant
{
    // TODO: use base class (see UpdateMerchantRequest)
    public class MerchantRequest
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(50, MinimumLength = 3)]
        public string BusinessName { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string MarketingName { get; set; }

        [StringLength(9, MinimumLength = 9)]
        public string BusinessID { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string ContactPerson { get; set; }
    }
}
