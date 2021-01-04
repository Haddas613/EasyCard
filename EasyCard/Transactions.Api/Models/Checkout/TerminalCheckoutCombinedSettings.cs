﻿using Merchants.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Checkout
{
    public class TerminalCheckoutCombinedSettings : TerminalCheckoutSettings
    {
        [StringLength(250)]
        public string DefaultChargeDescription { get; set; }

        public string MarketingName { get; set; }

        public Guid? TerminalID { get; set; }

        public decimal? VATRate { get; set; }

        public string MerchantLogo { get; set; }
    }
}
