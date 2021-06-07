using IdentityServer.Resources;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models
{
    public class LoginInputModel
    {
        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "UsernameRequired")]
        public string Username { get; set; }

        [Required(ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "PasswordRequired")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}