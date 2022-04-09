using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum BillingDealOperationCodesEnum : short
    {
        Created = 0,
        Updated = 1,
        Paused = 2,
        Unpaused = 3,
        Deactivated = 4,
        CreditCardTokenChanged = 5,
        CreditCardTokenRemoved = 6,
        Activated = 7,
        ExpirationEmailSent = 8
    }
}
