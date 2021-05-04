using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared.Business.Audit
{
    public enum OperationCodesEnum : short
    {
        [EnumMember(Value = "merchantUpdated")]
        MerchantUpdated = 0,

        [EnumMember(Value = "merchantCreated")]
        MerchantCreated = 1,

        [EnumMember(Value = "loggedIn")]
        LoggedIn = 2,

        [EnumMember(Value = "loggedOut")]
        LoggedOut = 3,

        [EnumMember(Value = "loginCreated")]
        LoginCreated = 4,

        [EnumMember(Value = "invitationSent")]
        InvitationSent = 5,

        [EnumMember(Value = "adminResetedPassword")]
        AdminResetedPassword = 6,

        [EnumMember(Value = "userResetedPassword")]
        UserResetedPassword = 7,

        [EnumMember(Value = "userSetNewPassword")]
        UserSetNewPassword = 8,

        [EnumMember(Value = "userEnabledTwoFactor")]
        UserEnabledTwoFactor = 9,

        [EnumMember(Value = "userDisabledTwoFactor")]
        UserDisabledTwoFactor = 10,

        [EnumMember(Value = "accountLocked")]
        AccountLocked = 11,

        [EnumMember(Value = "accountUnlocked")]
        AccountUnlocked = 12,

        [EnumMember(Value = "terminalCreated")]
        TerminalCreated = 13,

        [EnumMember(Value = "terminalUpdated")]
        TerminalUpdated = 14,

        [EnumMember(Value = "userTerminalLinkAdded")]
        UserTerminalLinkAdded = 15,

        [EnumMember(Value = "userTerminalLinkRemoved")]
        UserTerminalLinkRemoved = 16,

        [EnumMember(Value = "terminalApiKeyChanged")]
        TerminalApiKeyChanged = 17
    }
}
