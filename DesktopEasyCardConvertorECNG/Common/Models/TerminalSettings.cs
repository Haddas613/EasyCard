using Common.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace Common.Models
{
    public class TerminalSettings
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

        public bool? EnableCancellationOfUntransmittedTransactions { get; set; }

        public bool? NationalIDRequired { get; set; }

        public bool? CvvRequired { get; set; }

        public bool? J2Allowed { get; set; }

        public bool? J5Allowed { get; set; }

        public int J5ExpirationDays { get; set; }

        public bool? SendTransactionSlipEmailToMerchant { get; set; }

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

        public TerminalTransmissionScheduleEnum? TransmissionSchedule { get; set; }

        public bool? UseQuickChargeByDefault { get; set; }

        /// <summary>
        /// if 0 takes values from BankOfIsrael
        /// </summary>
        public decimal EuroRate { get; set; }

        public decimal DollarRate { get; set; }

        public TerminalSettings()
        {
            J5ExpirationDays = 1;
        }

        public bool DoNotCreateSaveTokenInitialDeal { get; set; }
    }
}
