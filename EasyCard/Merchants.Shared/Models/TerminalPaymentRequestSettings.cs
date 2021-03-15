using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Merchants.Shared.Models
{
    public class TerminalPaymentRequestSettings
    {
        [StringLength(100)]
        public string FromAddress { get; set; }

        [StringLength(250)]
        public string DefaultRequestSubject { get; set; }

        [StringLength(50)]
        public string EmailTemplateCode { get; set; }

        [StringLength(250)]
        public string MerchantLogo { get; set; }
    }
}
