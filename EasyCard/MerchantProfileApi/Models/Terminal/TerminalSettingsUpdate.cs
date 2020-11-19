using Merchants.Shared.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Terminal
{
    // set of settings which can be changed by merchant (some settings can be updated only by admin - see TerminalSettings.cs)
    public class TerminalSettingsUpdate
    {
        [StringLength(250)]
        public string DefaultChargeDescription { get; set; }

        [StringLength(250)]
        public string DefaultRefundDescription { get; set; }

        [StringLength(250)]
        public string DefaultItemName { get; set; }

        public decimal? VATRate { get; set; }
    }
}
