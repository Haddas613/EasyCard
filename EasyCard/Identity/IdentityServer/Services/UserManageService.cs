using IdentityServer.Helpers;
using IdentityServer.Models;
using IdentityServerClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

            if (model.Roles == null)
            {
                model.Roles = new List<string>();
            }

            if (!model.Roles.Any(r => r != Roles.Merchant))
            {
                model.Roles.Add(Roles.Merchant);
            }

            foreach (var role in model.Roles.Distinct())
            {
                await userManager.AddToRoleAsync(user, role);
            }

            await userManager.AddClaim(allClaims, user, "extension_MerchantID", model.MerchantID);

            logger.LogInformation("User created a new account");
            return result;
        }

        public async Task<bool> UpdateUser(UpdateUserRequestModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserID);

            if (user == null)
            {
                logger.LogError($"User is not found. User id: {model.UserID}");

                return false;
            }

            if (model.Roles == null)
            {
                model.Roles = new HashSet<string>();
            }

            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                user.PhoneNumber = model.PhoneNumber;
            }

            if (!string.IsNullOrWhiteSpace(model.FirstName) || !string.IsNullOrWhiteSpace(model.LastName))
            {
                var claims = await userManager.GetClaimsAsync(user);

                if (!string.IsNullOrWhiteSpace(model.FirstName))
                {
                    var fnClaim = claims.FirstOrDefault(c => c.Type == Claims.FirstNameClaim);

                    if (fnClaim != null)
                    {
                        await userManager.ReplaceClaimAsync(user, fnClaim, new Claim(Claims.FirstNameClaim, model.FirstName));
                    }
                    else
                    {
                        await userManager.AddClaim(claims, user, Claims.FirstNameClaim, model.FirstName);
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.LastName))
                {
                    var lnClaim = claims.FirstOrDefault(c => c.Type == Claims.LastNameClaim);

                    if (lnClaim != null)
                    {
                        await userManager.ReplaceClaimAsync(user, lnClaim, new Claim(Claims.LastNameClaim, model.LastName));
                    }
                    else
                    {
                        await userManager.AddClaim(claims, user, Claims.LastNameClaim, model.LastName);
                    }
                }
            }

            if (!model.Roles.Any(r => r != Roles.Merchant))
            {
                model.Roles.Add(Roles.Merchant);
            }

            foreach (var role in model.Roles.Distinct())
            {
                await userManager.AddToRoleAsync(user, role);
            }

            await userManager.UpdateAsync(user);

            return true;
        }
    }
}
