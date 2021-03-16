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

        Task RegisterSetPassword(ApplicationUser user);

        Task RegisterResetPassword(ApplicationUser user);

        Task RegisterChangePassword(ApplicationUser user);

        Task RegisterLogin(ApplicationUser user);

        Task RegisterLogout(ApplicationUser user);

        Task RegisterConfirmEmail(ApplicationUser user, string fullName);

        Task RegisterTwoFactorCompleted(ApplicationUser user);

        Task RegisterAction(ApplicationUser user, AuditingTypeEnum type);
    }
}
