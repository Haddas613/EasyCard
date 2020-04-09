using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Terminal
{
    public class TerminalSettings
    {
        public JObject RedirectPageSettings { get; set; }

        public JObject PaymentButtonSettings { get; set; }

        public int? MinInstallments { get; set; }

        /// <summary>
        /// 0 means installments blocked
        /// </summary>
        public int? MaxInstallments { get; set; }

        public int? MinCreditInstallments { get; set; }

        public int? MaxCreditInstallments { get; set; }

        public bool EnableDeletionOfUntransmittedTransactions { get; set; }

        public bool NationalIDRequired { get; set; }

        public bool CvvRequired { get; set; }
    }
}
