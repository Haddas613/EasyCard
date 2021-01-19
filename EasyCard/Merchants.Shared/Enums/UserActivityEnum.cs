using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Shared.Enums
{
    public enum UserActivityEnum : short
    {
        Locked = -1,
        LoggedIn = 0,
        Unlocked,
        ResetPassword,
        SetTwoFactorAuth,
        PhoneNumberChanged
    }
}
