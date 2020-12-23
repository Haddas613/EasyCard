using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Merchants.Shared.Enums
{
    public enum UserStatusEnum : short
    {
        [EnumMember(Value = "Invited")]
        Invited = 0,

        [EnumMember(Value = "Locked")]
        Locked = -1,

        [EnumMember(Value = "Active")]
        Active = 1,
    }
}
