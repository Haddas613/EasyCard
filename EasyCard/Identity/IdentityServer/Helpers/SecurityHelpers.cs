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
        public static async Task AddClaim(this UserManager<ApplicationUser> userManager, IList<Claim> allClaims, ApplicationUser user, string type, string value, bool additionalValue = false)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (additionalValue)
                {
                    var claim = allClaims.FirstOrDefault(c => c.Type == type && c.Value == value);
                    if (claim == null)
                    {
                        await userManager.AddClaimAsync(user, new Claim(type, value));
                    }
                }
                else
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

        public static async Task RemoveClaim(this UserManager<ApplicationUser> userManager, IList<Claim> allClaims, ApplicationUser user, string type)
        {
            var claims = allClaims.Where(c => c.Type == type).ToList();
            if (claims?.Count() > 0)
            {
                await userManager.RemoveClaimsAsync(user, claims);
            }
        }
    }
}
