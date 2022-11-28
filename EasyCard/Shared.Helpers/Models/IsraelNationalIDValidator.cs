using Shared.Helpers.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared.Helpers.Models
{
    public class IsraelNationalIDValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var nationalID = value as string;

            if (nationalID is null)
            {
                return ValidationResult.Success;
            }

            if (!IsraelNationalIdHelpers.Valid(nationalID))
            {
                return new ValidationResult(Messages.ConsumerNationalIDInvalid);
            }

            return ValidationResult.Success;
        }
    }
}
