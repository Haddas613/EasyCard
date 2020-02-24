using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Terminal
{
    public enum TerminalStatusEnum
    {
        PendingApproval = 0,
        Approved = 1,
        Disabled = -1
    }
}
