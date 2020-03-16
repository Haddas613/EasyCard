using IdentityServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Security.Auditing
{
    public interface IAuditLogger
    {
        Task ValidateUser(string email, string bankAccount);

        Task RegisterForgotPassword(string email);

        Task RegisterSetPassword(string email);

        Task RegisterLogin(ApplicationUser user);

        Task RegisterLogout(ApplicationUser user);

        Task RegisterConfirmEmail(ApplicationUser user);
    }
}
