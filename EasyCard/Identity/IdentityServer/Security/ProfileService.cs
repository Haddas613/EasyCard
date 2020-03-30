using IdentityModel;
using IdentityServer.Models;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Security
{
    public class ProfileService : IProfileService
    {
        protected UserManager<ApplicationUser> userManager;

        public ProfileService(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            //>Processing
            var user = await userManager.GetUserAsync(context.Subject);

            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Name, user.UserName),
            };

            context.IssuedClaims.AddRange(claims);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            //>Processing
            var user = await userManager.GetUserAsync(context.Subject);

            context.IsActive = (user != null); //&& user.IsActive;
        }
    }
}
