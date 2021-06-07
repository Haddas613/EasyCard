using IdentityServer.Resources;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "Required")]
        [EmailAddress(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "EmailValidationMessage")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "Required")]
        [MaxLength(100)]
        public string BankAccount { get; set; }

        public bool CheckBankAccount { get; set; }
    }
}
