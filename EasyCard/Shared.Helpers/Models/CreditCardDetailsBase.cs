using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared.Helpers
{
    public class CreditCardDetailsBase
    {
        [Required]
        [StringLength(19, MinimumLength = 10)]
        public string CardNumber { get; set; }

        [Required]
        public CardExpiration CardExpiration { get; set; }

        public string CardVendor { get; set; }

        public string CardOwnerName { get; set; }

        public string CardOwnerNationalID { get; set; }
    }
}
