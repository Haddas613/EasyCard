using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Shared.Enums
{
    public enum UserStatusEnum : short
    {
        Invited = 0,
        Locked = -1,
        Active = 1,
    }
}
