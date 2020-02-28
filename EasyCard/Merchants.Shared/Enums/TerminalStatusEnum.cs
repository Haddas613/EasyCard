using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Shared.Enums
{
    public enum TerminalStatusEnum
    {
        PendingApproval = 0,
        Approved = 1,
        Disabled = -1,
    }
}
