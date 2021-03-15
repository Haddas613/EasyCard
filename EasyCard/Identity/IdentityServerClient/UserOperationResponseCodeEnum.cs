using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServerClient
{
    public enum UserOperationResponseCodeEnum : short
    {
        UnknwnError = 0,
        UserCreated = 1,
        UserUpdated = 2,
        UserAlreadyExists = -3,
        UserNotFound = -4,
        UserDeleted = 5,
        PasswordReseted = 5,
        InvitationResent = 6,
        UserLocked = 7,
        UserUnlocked = 8,
        UserUnlinkedFromMerchant = 9
    }
}
