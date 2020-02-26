using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(100)]
        public string BankAccount { get; set; }

        public bool CheckBankAccount { get; set; }
    }
}
