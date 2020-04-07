using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Merchants.Shared.Enums
{
    public enum TerminalStatusEnum
    {
        [EnumMember(Value = "pendingApproval")]
        PendingApproval = 0,

        [EnumMember(Value = "approved")]
        Approved = 1,

        [EnumMember(Value = "disabled")]
        Disabled = -1,
    }
}
