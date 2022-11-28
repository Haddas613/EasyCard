using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Merchants.Shared.Models.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class NoSpecialCharsAttribute : ValidationAttribute
    {
        private static readonly Regex SpecChars = new Regex(@"\`|\~|\@|\#|\$|\%|\^|\(|\)|\=|\[|\{|\]|\}|\|\'|\<|\>|\/|\;");

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (SpecChars.Match(value.ToString()).Success)
            {
                return new ValidationResult(Messages.SpecialCharactersNotAllowed);
            }

            return ValidationResult.Success;
        }
    }
}