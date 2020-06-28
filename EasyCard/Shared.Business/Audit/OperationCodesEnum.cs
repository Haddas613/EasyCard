using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Business.Audit
{
    public enum OperationCodesEnum : short
    {
        MerchantUpdated,
        MerchantCreated,

        LoggedIn,
        LoggedOut,
        LoginCreated,
        InvitationSent,
        AdminResetedPassword,

        UserResetedPassword,
        UserSetNewPassword,
        UserEnabledTwoFactor,
        UserDisabledTwoFactor,

        AccountLocked,
        AccountUnlocked,

        TerminalCreated,
        TerminalUpdated,

        UserTerminalLinkAdded,
        UserTerminalLinkRemoved
    }
}
