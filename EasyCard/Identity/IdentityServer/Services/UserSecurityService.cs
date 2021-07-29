using IdentityServer.Data;
using IdentityServer.Models;
using IdentityServer.Models.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Services
{
    public class UserSecurityService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;
        private readonly SecuritySettings securitySettings;
        private readonly ILogger<UserSecurityService> logger;

        public UserSecurityService(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IOptions<SecuritySettings> securitySettings,
            ILogger<UserSecurityService> logger)
        {
            this.userManager = userManager;
            this.context = context;
            this.securitySettings = securitySettings.Value;
            this.logger = logger;
        }

        /// <summary>
        /// Sets new password for user if password was not used before (based on SecuritySettings). Returns false if password was not updated.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newPassword"></param>
        public async Task<bool> TrySetNewPassword(ApplicationUser user, string newPassword)
        {
            var previousPasswords = await context.UserPasswordSnapshots.Where(s => s.UserId == user.Id).Take(securitySettings.RememberLastPasswords).ToListAsync();

            //Password is currently in use
            if (userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, newPassword) == PasswordVerificationResult.Success)
            {
                return false;
            }

            //Password was used before
            foreach (var previousPassword in previousPasswords)
            {
                //Create a fake user for PasswordHasher
                var stub = new ApplicationUser { PasswordHash = previousPassword.HashedPassword, SecurityStamp = previousPassword.SecurityStamp };

                if (userManager.PasswordHasher.VerifyHashedPassword(stub, previousPassword.HashedPassword, newPassword) == PasswordVerificationResult.Success)
                {
                    return false;
                }
            }

            user.PasswordHash = userManager.PasswordHasher.HashPassword(user, newPassword);
            user.PasswordUpdated = DateTime.UtcNow;
            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                logger.LogError(string.Join(";", result.Errors.Select(e => $"{e.Code}:{e.Description}")));
                throw new Exception("Something went wrong. Contact administration");
            }

            if (previousPasswords.Count >= securitySettings.RememberLastPasswords)
            {
                context.UserPasswordSnapshots.RemoveRange(
                        previousPasswords.OrderByDescending(p => p.UserPasswordSnapshotID).Take(previousPasswords.Count - (securitySettings.RememberLastPasswords - 1)));
                await context.SaveChangesAsync();
            }

            context.UserPasswordSnapshots.Add(new Data.Entities.UserPasswordSnapshot
            {
                Created = DateTime.UtcNow,
                UserId = user.Id,
                HashedPassword = user.PasswordHash,
                SecurityStamp = user.SecurityStamp
            });

            await context.SaveChangesAsync();

            return true;
        }
    }
}
