using IdentityServer.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System;

namespace IdentityServer.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public DateTime? PasswordUpdated { get; set; }
    }
}
