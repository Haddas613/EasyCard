using IdentityServer.Resources;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "Required")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "Required")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "Required")]
        [Compare("NewPassword", ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "PasswordMatchValidatonMessage")]
        public string ConfirmPassword { get; set; }

        [BindNever]
        public bool Success { get; set; }
    }
}
