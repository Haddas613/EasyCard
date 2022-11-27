using Shared.Helpers.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared.Helpers.Models
{
    public class CardExpirationValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var expiration = value as CardExpiration;

            if (expiration is null)
            {
                return new ValidationResult(Messages.CreditCardExpirationRequired);
            }

            if (expiration.ToDate().Value < DateTime.UtcNow)
            {
                return new ValidationResult(Messages.CreditCardExpired);
            }

            return ValidationResult.Success;
        }
    }
}
