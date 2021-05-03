using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Security.Auditing
{
    public enum AuditingTypeEnum
    {
        Unknown,

        EmailConfirmed,

        PasswordResetted,
        PasswordSet,
        PasswordForgot,

        UserValidated,

        LoggedIn,
        LoggedOut,

        UserEnabledTwoFactor,
        PasswordChanged,
        UserDisabledTwoFactor,

        LockedOut
    }
}
