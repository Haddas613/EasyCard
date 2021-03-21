using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace Shared.Helpers.Models.Attributes
{
    /// <summary>
    /// Provides <see cref="System.ComponentModel.DataAnnotations.CreditCardAttribute"/> functionality with added support of Diarect Credit card
    /// </summary>
    public class CreditCardPlusAttribute : System.ComponentModel.DataAnnotations.ValidationAttribute
    {
        public bool Required { get; set; }

        public override bool IsValid(object value)
        {
            string val = value as string;

            if (val == null)
            {
                return !Required;
            }

            if (string.IsNullOrWhiteSpace(val))
            {
                return false;
            }

            var ccAttribute = new CreditCardAttribute();

            if (ccAttribute.IsValid(val))
            {
                return true;
            }

            // Diarect support
            if (Regex.IsMatch(val, "^[0-9]{9}$"))
            {
                return true;
            }

            return false;
        }
    }
}
