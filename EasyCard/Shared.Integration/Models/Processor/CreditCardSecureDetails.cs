using Newtonsoft.Json;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared.Integration.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CreditCardSecureDetails : CreditCardDetailsBase
    {
        [Required]
        [StringLength(4, MinimumLength = 3)]
        public string Cvv { get; set; }

        /// <summary>
        /// after code 3 or 4 user can insert this value from credit company
        /// </summary>
        [StringLength(10)]
        public string AuthNum { get; set; }
    }
}
