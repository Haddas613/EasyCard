using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Security
{
    public interface IAuditLogger
    {
        Task ValidateUser(string email, string bankAccount);

        Task RegisterForgotPassword(string email);

        Task RegisterSetPassword(string email);
    }
}
