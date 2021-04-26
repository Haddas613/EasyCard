using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Helpers
{
    public class UserHelpers
    {
        private readonly UserManager<ApplicationUser> userManager;

        private string currentUserFullName = null;

        public UserHelpers(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<string> GetUserFullname(ApplicationUser user)
        {
            if (currentUserFullName != null)
            {
                return currentUserFullName;
            }

            var claims = await userManager.GetClaimsAsync(user);

            var fnClaim = claims.FirstOrDefault(c => c.Type == Claims.FirstNameClaim);
            var lnClaim = claims.FirstOrDefault(c => c.Type == Claims.LastNameClaim);

            return currentUserFullName = $"{fnClaim?.Value} {lnClaim?.Value}".Trim();
        }

        public string GetUserFullName(string firstName, string lastName) => $"{firstName} {lastName}".Trim();
    }
}
