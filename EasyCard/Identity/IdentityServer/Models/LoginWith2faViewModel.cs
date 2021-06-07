using IdentityServer.Models.Enums;
using IdentityServer.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models
{
    public class LoginWith2faViewModel
    {
        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "Required")]
        [StringLength(8, MinimumLength = 6, ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "StringLengthValidationMessage")]
        [DataType(DataType.Text)]
        public string TwoFactorCode { get; set; }

        public TwoFactorAuthTypeEnum LoginType { get; set; }
    }
}
