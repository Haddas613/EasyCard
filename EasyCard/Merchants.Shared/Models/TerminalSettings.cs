using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Merchants.Shared.Models
{
    // NOTE: set of settings which can be changed by admin (some settings can be also updated by merchant - see TerminalSettingsUpdate.cs)
    public class TerminalSettings
    {
        [Range(1, 100)]
        public int? MinInstallments { get; set; }

        /// <summary>
        /// If we set it to zero means installments blocked
        /// </summary>
        [Range(0, 100)]
        public int? MaxInstallments { get; set; }

        [Range(1, 100)]
        public int? MinCreditInstallments { get; set; }

        [Range(0, 100)]
        public int? MaxCreditInstallments { get; set; }

        public bool? EnableCancellationOfUntransmittedTransactions { get; set; }

        public bool? NationalIDRequired { get; set; }

        public bool? CvvRequired { get; set; }

        public bool? J2Allowed { get; set; }

        public bool? J5Allowed { get; set; }

        [StringLength(250)]
        public string DefaultChargeDescription { get; set; }

        [StringLength(250)]
        public string DefaultRefundDescription { get; set; }

        [StringLength(250)]
        public string DefaultItemName { get; set; }

        [StringLength(50)]
        public string DefaultSKU { get; set; }

        public decimal? VATRate
        {
            get
            {
                return VATExempt ? 0 : VATRateGlobal;
            }
        }

        [JsonIgnore]
        public decimal VATRateGlobal { get; set; }

        public bool VATExempt { get; set; }
    }
}
