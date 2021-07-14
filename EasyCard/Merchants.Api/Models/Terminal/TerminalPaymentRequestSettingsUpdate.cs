using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Terminal
{
    public class TerminalPaymentRequestSettingsUpdate
    {
        [StringLength(100)]
        public string FromAddress { get; set; }

        [StringLength(250)]
        public string DefaultRequestSubject { get; set; }

        [StringLength(250)]
        public string DefaultRefundRequestSubject { get; set; }

        [StringLength(50)]
        public string EmailTemplateCode { get; set; }
    }
}
