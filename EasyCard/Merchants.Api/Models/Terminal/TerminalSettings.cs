using Merchants.Api.Models.Merchant;
using Merchants.Api.Models.User;
using Merchants.Shared.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Terminal
{
    public class TerminalSettings
    {
        public int? MinInstallments { get; set; }

        /// <summary>
        /// If we set it to zero means installments blocked
        /// </summary>
        [Range(0, int.MaxValue)]
        public int? MaxInstallments { get; set; }

        [Range(0, int.MaxValue)]
        public int? MinCreditInstallments { get; set; }

        [Range(0, int.MaxValue)]
        public int? MaxCreditInstallments { get; set; }

        public bool EnableDeletionOfUntransmittedTransactions { get; set; }

        public bool NationalIDRequired { get; set; }

        public bool CvvRequired { get; set; }
    }
}
