using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Terminal
{
    public class TerminalRequest
    {
        [Required]
        public Guid MerchantID { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(50, MinimumLength = 6)]
        public string Label { get; set; }

        [Required]
        public TerminalSettings Settings { get; set; }

        public TerminalBillingSettings BillingSettings { get; set; }
    }
}
