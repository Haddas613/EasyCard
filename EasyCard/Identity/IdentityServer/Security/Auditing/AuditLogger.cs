﻿using IdentityServer.Data;
using IdentityServer.Data.Entities;
using IdentityServer.Models;
using IdentityServer.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

        public AuditLogger(ApplicationDbContext context, IHttpContextAccessor accessor)
        {
            this.context = context;
            this.accessor = accessor;
        }

        public async Task RegisterConfirmEmail(ApplicationUser user)
        {
            var audit = await GetAudit(user, AuditingTypeEnum.EmailConfirmed);

            context.UserAudits.Add(audit);
            await context.SaveChangesAsync();
        }

        public async Task RegisterForgotPassword(string email)
        {
            var audit = await GetAudit(email, AuditingTypeEnum.PasswordForgot);

            await SaveAudit(audit);
        }

        public async Task RegisterLogin(ApplicationUser user)
        {
            var audit = await GetAudit(user, AuditingTypeEnum.LoggedIn);

            await SaveAudit(audit);
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
        }

        private async Task SaveAudit(UserAudit audit)
        {
            context.UserAudits.Add(audit);
            await context.SaveChangesAsync();
        }

        private async Task<UserAudit> GetAudit(ApplicationUser user, AuditingTypeEnum type) => await GetAudit(user.Email, type, user);

        private async Task<UserAudit> GetAudit(string email, AuditingTypeEnum type, ApplicationUser user = null)
        {
            if (user == null)
            {
                user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }

            return new UserAudit
            {
                Email = email,
                OperationCode = type.ToString(),
                UserId = user?.Id,
                SourceIP = accessor.HttpContext.Connection?.RemoteIpAddress?.ToString(),
                OperationDate = DateTime.UtcNow
            };
        }
    }
}