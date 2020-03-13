using IdentityServer.Data;
using IdentityServer.Models;
using IdentityServer.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Security.Auditing
{
    public class AuditLogger : IAuditLogger
    {
        private readonly ApplicationDbContext context;

        public AuditLogger(ApplicationDbContext context)
        {
            this.context = context;
        }

        public Task RegisterConfirmEmail(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task RegisterForgotPassword(string email)
        {
            //throw new NotImplementedException();
            return Task.FromResult(true);
        }

        public Task RegisterLogin(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task RegisterLogout(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task RegisterSetPassword(string email)
        {
            //throw new NotImplementedException();
            return Task.FromResult(true);
        }

        public Task ValidateUser(string email, string bankAccount)
        {
            //throw new NotImplementedException();
            return Task.FromResult(true);
        }
    }
}
