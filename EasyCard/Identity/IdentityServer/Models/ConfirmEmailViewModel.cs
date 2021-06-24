using IdentityServer.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models
{
    public class ConfirmEmailViewModel
    {
        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "Required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&^])[A-Za-z\d@$!%*#?&^]{8,}$", ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "PasswordValidationMessage")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "PasswordMatchValidationMessage")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "Required")]
        [Phone]
        [StringLength(50)]
        public string PhoneNumber { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "Required")]
        [StringLength(100, MinimumLength = 2)]
        public string LastName { get; set; }
    }
}
