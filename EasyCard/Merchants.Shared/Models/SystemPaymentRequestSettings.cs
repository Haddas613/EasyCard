using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Merchants.Shared.Models
{
    public class SystemPaymentRequestSettings
    {
        [StringLength(100)]
        public string FromAddress { get; set; }

        [StringLength(250)]
        public string DefaultRequestSubject { get; set; }
    }
}
