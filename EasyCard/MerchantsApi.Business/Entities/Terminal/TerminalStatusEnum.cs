using System;
using System.Collections.Generic;
using System.Text;

namespace MerchantsApi.Business.Entities
{
    public enum TerminalStatusEnum
    {
        PendingApproval = 0,
        Approved = 1,
        Disabled = -1
    }
}
