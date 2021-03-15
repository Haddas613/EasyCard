using IdentityServer.Helpers;
using IdentityServer.Models;
using IdentityServerClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Services
{
    public class UserManageService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<UserManageService> logger;

        public UserManageService(UserManager<ApplicationUser> userManager, ILogger<UserManageService> logger)
        {
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task<IdentityResult> CreateUser(CreateUserRequestModel model)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.CellPhone, LockoutEnabled = true };
            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                logger.LogInformation("User is not created");

                return result;
            }

            var allClaims = await userManager.GetClaimsAsync(user);

            await userManager.AddClaim(allClaims, user, "extension_MerchantID", model.MerchantID);
            await userManager.AddToRoleAsync(user, "Merchant");

            logger.LogInformation("User created a new account");
            return result;
        }
    }
}
