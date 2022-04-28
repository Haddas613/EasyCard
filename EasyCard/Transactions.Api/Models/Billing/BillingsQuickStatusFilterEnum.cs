using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Billing
{
    public enum BillingsQuickStatusFilterEnum
    {
        All = 0,
        Completed = 1,
        Inactive = 2,
        Failed = 3,
        CardExpired = 4,
        TriggeredTomorrow = 5,
        Paused = 6,
        ExpiredNextMonth = 7,
        ManualTrigger = 8,
        InProgress = 9,
    }
}
