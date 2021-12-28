using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Helpers
{
    public static class SecurityHelpers
    {
        [Obsolete]
        public static async Task AddClaim(this UserManager<ApplicationUser> userManager, IList<Claim> allClaims, ApplicationUser user, string type, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var claim = allClaims.FirstOrDefault(c => c.Type == type);
                if (claim == null)
                {
                    await userManager.AddClaimAsync(user, new Claim(type, value));
                }
                else if (claim.Value != value)
                {
                    await userManager.RemoveClaimAsync(user, claim);
                    await userManager.AddClaimAsync(user, new Claim(type, value));
                }
            }
        }
    }
}
