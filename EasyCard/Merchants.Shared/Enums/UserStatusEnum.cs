using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Merchants.Shared.Enums
{
    public enum UserStatusEnum : short
    {
        [EnumMember(Value = "invited")]
        Invited = 0,

        [EnumMember(Value = "locked")]
        Locked = -1,

        [EnumMember(Value = "active")]
        Active = 1,
    }
}
