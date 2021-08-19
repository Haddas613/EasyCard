using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Merchants.Shared.Models
{
    public class SystemGlobalSettings
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
        public int J5ExpirationDays { get; set; }

        [StringLength(250)]
        public string DefaultChargeDescription { get; set; }

        [StringLength(250)]
        public string DefaultRefundDescription { get; set; }

        [StringLength(250)]
        public string DefaultItemName { get; set; }

        [StringLength(50)]
        public string DefaultSKU { get; set; }

        [Range(0, 1)]
        public decimal? VATRate { get; set; }
    }
}
