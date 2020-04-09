﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Merchant
{
    public class UpdateMerchantRequest
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(50, MinimumLength = 6)]
        public string BusinessName { get; set; }

        [StringLength(50, MinimumLength = 6)]
        public string MarketingName { get; set; }

        [StringLength(50)]
        public string BusinessID { get; set; }

        [StringLength(50, MinimumLength = 6)]
        public string ContactPerson { get; set; }
    }
}
