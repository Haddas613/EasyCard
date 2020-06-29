using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared.Helpers
{
    public class CreditCardDetailsBase
    {
        [Required]
        [StringLength(19, MinimumLength = 9)]
        public string CardNumber { get; set; }

        [Required]
        [JsonConverter(typeof(CardExpirationConverter))]
        public CardExpiration CardExpiration { get; set; }

        [StringLength(20)]
        public string CardVendor { get; set; }

        [StringLength(50, MinimumLength = 2)]
        public string CardOwnerName { get; set; }

        [StringLength(20)]
        public string CardOwnerNationalID { get; set; }

        [StringLength(39)]
        [RegularExpression(@"^;\d{16}=\d{20}\?$")]
        public string CardReaderInput { get; set; }
    }
}
