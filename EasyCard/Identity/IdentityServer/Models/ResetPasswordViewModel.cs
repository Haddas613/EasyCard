using IdentityServer.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "Required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "StringLengthValidationMessage")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&^])[A-Za-z\d@$!%*#?&^]{8,}$", ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "PasswordValidationMessage")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "PasswordMatchValidatonMessage")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}
