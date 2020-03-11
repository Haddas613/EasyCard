using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Security
{
    public class AuditLogger : IAuditLogger
    {
        public Task RegisterForgotPassword(string email)
        {
            throw new NotImplementedException();
        }

        public Task RegisterSetPassword(string email)
        {
            throw new NotImplementedException();
        }

        public Task ValidateUser(string email, string bankAccount)
        {
            throw new NotImplementedException();
        }
    }
}
