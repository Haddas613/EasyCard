using System.ComponentModel.DataAnnotations;
using IdentityServer.Resources;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IdentityServer.Models
{
    public class VerifyAuthentificatorCodeViewModel
    {
        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "Required")]
        [StringLength(8, MinimumLength = 6, ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "StringLengthValidationMessage")]
        [DataType(DataType.Text)]
        public string Code { get; set; }

        [BindNever]
        public string PhoneNumber { get; set; }
    }
}
