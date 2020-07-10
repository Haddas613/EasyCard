using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum BillingDealStatusEnum : short
    {
        Initial = 0,
        Active = 1,
        Inactive = -1
    }
}
