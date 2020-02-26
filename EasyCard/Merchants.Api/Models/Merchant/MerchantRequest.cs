﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Merchant
{
    public class MerchantRequest
    {
        [Required]
        public string BusinessName { get; set; }
    }
}
