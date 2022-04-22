using IdentityServer.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models
{
    public class ForceUpdatePasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "PasswordRequired")]
        [DataType(DataType.Password)]
        [StringLength(32, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&^])[A-Za-z\d@$!%*#?&^]{8,}$", ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "PasswordValidationMessage")]
        public string NewPassword { get; set; }

        public string ReturnUrl { get; set; }
    }
}
