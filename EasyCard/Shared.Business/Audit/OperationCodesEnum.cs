using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared.Business.Audit
{
    public enum OperationCodesEnum : short
    {
        [EnumMember(Value = "merchantUpdated")]
        MerchantUpdated,
        [EnumMember(Value = "merchantCreated")]
        MerchantCreated,
        [EnumMember(Value = "loggedIn")]
        LoggedIn,
        [EnumMember(Value = "loggedOut")]
        LoggedOut,
        [EnumMember(Value = "loginCreated")]
        LoginCreated,
        [EnumMember(Value = "invitationSent")]
        InvitationSent,
        [EnumMember(Value = "adminResetedPassword")]
        AdminResetedPassword,

        [EnumMember(Value = "userResetedPassword")]
        UserResetedPassword,
        [EnumMember(Value = "userSetNewPassword")]
        UserSetNewPassword,
        [EnumMember(Value = "userEnabledTwoFactor")]
        UserEnabledTwoFactor,
        [EnumMember(Value = "userDisabledTwoFactor")]
        UserDisabledTwoFactor,

        [EnumMember(Value = "accountLocked")]
        AccountLocked,
        [EnumMember(Value = "accountUnlocked")]
        AccountUnlocked,

        [EnumMember(Value = "terminalCreated")]
        TerminalCreated,
        [EnumMember(Value = "terminalUpdated")]
        TerminalUpdated,

        [EnumMember(Value = "userTerminalLinkAdded")]
        UserTerminalLinkAdded,
        [EnumMember(Value = "userTerminalLinkRemoved")]
        UserTerminalLinkRemoved,
        [EnumMember(Value = "terminalApiKeyChanged")]
        TerminalApiKeyChanged
    }
}
