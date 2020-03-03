using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Merchant
{
    public class UpdateMerchantRequest
    {
        [Required(AllowEmptyStrings = false)]
        [MinLength(6)]
        public string BusinessName { get; set; }

        [MinLength(6)]
        public string MarketingName { get; set; }

        public string BusinessID { get; set; }

        [MinLength(6)]
        public string ContactPerson { get; set; }
    }
}
