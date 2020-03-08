using IdentityServer.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Services
{
    public class AuditLogger : IAuditLogger
    {
        public Task RegisterForgotPassword(string email)
        {
            //throw new NotImplementedException();
            return Task.FromResult(true);
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
