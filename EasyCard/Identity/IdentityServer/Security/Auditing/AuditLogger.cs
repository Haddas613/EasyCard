using IdentityServer.Data;
using IdentityServer.Data.Entities;
using IdentityServer.Models;
using IdentityServer.Security;
using Merchants.Api.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Security.Auditing
{
    public class AuditLogger : IAuditLogger
    {
        private readonly ApplicationDbContext context;
        private readonly IHttpContextAccessor accessor;
        private readonly IMerchantsApiClient merchantsApiClient;
        private readonly ILogger logger;

        public AuditLogger(ApplicationDbContext context, IHttpContextAccessor accessor, IMerchantsApiClient merchantsApiClient, ILogger<AuditLogger> logger)
        {
            this.context = context;
            this.accessor = accessor;
            this.merchantsApiClient = merchantsApiClient;
            this.logger = logger;
        }

        public async Task RegisterConfirmEmail(ApplicationUser user, string fullName)
        {
            var audit = await GetAudit(user, AuditingTypeEnum.EmailConfirmed);

            context.UserAudits.Add(audit);
            await context.SaveChangesAsync();

            try
            {
                await merchantsApiClient.LogUserActivity(new Merchants.Api.Client.Models.UserActivityRequest
                {
                    UserActivity = Merchants.Shared.Enums.UserActivityEnum.EmailConfirmed,
                    UserID = user.Id,
                    DisplayName = fullName,
                    Email = user.Email
                });
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to register user action audit", ex);
            }
        }

        public async Task RegisterForgotPassword(string email)
        {
            var audit = await GetAudit(email, AuditingTypeEnum.PasswordForgot);

            await SaveAudit(audit);
        }

        public async Task RegisterLogin(ApplicationUser user, string fullName)
        {
            var audit = await GetAudit(user, AuditingTypeEnum.LoggedIn);

            await SaveAudit(audit);

            try
            {
                await merchantsApiClient.LogUserActivity(new Merchants.Api.Client.Models.UserActivityRequest
                {
                    UserActivity = Merchants.Shared.Enums.UserActivityEnum.LoggedIn,
                    UserID = user.Id,
                    Email = user.Email,
                    DisplayName = fullName
                });
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to register user action audit", ex);
            }
        }

        public async Task RegisterLockout(ApplicationUser user)
        {
            var audit = await GetAudit(user, AuditingTypeEnum.LockedOut);

            await SaveAudit(audit);

            try
            {
                await merchantsApiClient.LogUserActivity(new Merchants.Api.Client.Models.UserActivityRequest
                {
                    UserActivity = Merchants.Shared.Enums.UserActivityEnum.Locked,
                    UserID = user.Id,
                    Email = user.Email
                });
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to register user action audit", ex);
            }
        }

        public async Task RegisterLogout(ApplicationUser user)
        {
            var audit = await GetAudit(user, AuditingTypeEnum.LoggedOut);

            await SaveAudit(audit);
        }

        public async Task ValidateUser(string email, string bankAccount)
        {
            var audit = await GetAudit(email, AuditingTypeEnum.UserValidated);
            audit.OperationDescription = bankAccount;
            await SaveAudit(audit);
        }

        public async Task RegisterSetPassword(ApplicationUser user)
        {
            var audit = await GetAudit(user, AuditingTypeEnum.PasswordSet);

            await SaveAudit(audit);
        }

        public async Task RegisterResetPassword(ApplicationUser user)
        {
            var audit = await GetAudit(user, AuditingTypeEnum.PasswordResetted);

            await SaveAudit(audit);

            try
            {
                await merchantsApiClient.LogUserActivity(new Merchants.Api.Client.Models.UserActivityRequest
                {
                    UserActivity = Merchants.Shared.Enums.UserActivityEnum.ResetPassword,
                    UserID = user.Id,
                    DisplayName = user.UserName,
                    Email = user.Email
                });
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to register user action audit", ex);
            }
        }

        public async Task RegisterTwoFactorCompleted(ApplicationUser user)
        {
            var audit = await GetAudit(user, AuditingTypeEnum.UserEnabledTwoFactor);

            await SaveAudit(audit);

            try
            {
                await merchantsApiClient.LogUserActivity(new Merchants.Api.Client.Models.UserActivityRequest
                {
                    UserActivity = Merchants.Shared.Enums.UserActivityEnum.SetTwoFactorAuth,
                    UserID = user.Id,
                    DisplayName = user.UserName,
                    Email = user.Email,
                });
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to register user action audit", ex);
            }
        }

        private async Task SaveAudit(UserAudit audit)
        {
            try
            {
                if (audit != null)
                {
                    context.UserAudits.Add(audit);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to register user action audit", ex);
            }
        }

        private async Task<UserAudit> GetAudit(ApplicationUser user, AuditingTypeEnum type) => await GetAudit(user?.Email, type, user);

        private async Task<UserAudit> GetAudit(string email, AuditingTypeEnum type, ApplicationUser user = null)
        {
            if (user == null && !string.IsNullOrWhiteSpace(email))
            {
                user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }

            if (user != null)
            {
                return new UserAudit
                {
                    Email = email,
                    OperationCode = type.ToString(),
                    UserId = user?.Id,
                    SourceIP = accessor.HttpContext.Connection?.RemoteIpAddress?.ToString(),
                    OperationDate = DateTime.UtcNow
                };
            }
            else
            {
                // TODO: log error
                return null;
            }
        }

        public async Task RegisterChangePassword(ApplicationUser user)
        {
            var audit = await GetAudit(user, AuditingTypeEnum.PasswordChanged);

            await SaveAudit(audit);
        }

        public async Task RegisterAction(ApplicationUser user, AuditingTypeEnum type)
        {
            var audit = await GetAudit(user, type);

            await SaveAudit(audit);
        }
    }
}
