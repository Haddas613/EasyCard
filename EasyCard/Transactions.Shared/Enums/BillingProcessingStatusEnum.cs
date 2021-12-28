using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum BillingProcessingStatusEnum: short
    {
        Pending = 0,
        Started = 1,
        InProgress = 2
    }
}
