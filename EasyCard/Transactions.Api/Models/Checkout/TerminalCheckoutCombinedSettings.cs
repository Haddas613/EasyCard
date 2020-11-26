using Merchants.Shared.Models;
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
    }
}
