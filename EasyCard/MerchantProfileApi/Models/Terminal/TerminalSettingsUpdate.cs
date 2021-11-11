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
        [Range(1, 36)]
        public int? MinInstallments { get; set; }

        /// <summary>
        /// If we set it to zero means installments blocked
        /// </summary>
        [Range(0, 36)]
        public int? MaxInstallments { get; set; }

        [Range(1, 36)]
        public int? MinCreditInstallments { get; set; }

        [Range(0, 36)]
        public int? MaxCreditInstallments { get; set; }

        [StringLength(250)]
        public string DefaultChargeDescription { get; set; }

        [StringLength(250)]
        public string DefaultRefundDescription { get; set; }

        [StringLength(250)]
        public string DefaultItemName { get; set; }

        [StringLength(50)]
        public string DefaultSKU { get; set; }

        public bool VATExempt { get; set; }

        public bool? SendTransactionSlipEmailToMerchant { get; set; }

        public bool? UseQuickChargeByDefault { get; set; }

        /// <summary>
        /// if 0 takes values from BankOfIsrael
        /// </summary>
        public decimal EuroRate { get; set; }

        public decimal DollarRate { get; set; }
    }
}
