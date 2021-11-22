using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Common.Models
{
    public class CreditCardSecureDetails : CreditCardDetailsBase
    {
        [StringLength(4, MinimumLength = 3)]
        [RegularExpression("^[0-9]*$")]
        public string Cvv { get; set; }

        /// <summary>
        /// after code 3 or 4 user can insert this value from credit company
        /// </summary>
        [StringLength(10)]
        public string AuthNum { get; set; }
    }

}
