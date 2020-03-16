using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum RejectionReasonEnum
    {
        Unknown = 0,
        CreditCardIsMerchantsCard = 1,
        NationalIdIsMerchantsId = 2,
        SingleTransactionAmountExceeded = 3,
        DailyAmountExceeded = 4,
        CreditCardDailyUsageExceeded = 5,
        RefundNotMathRegularAmount = 6,
        RefundExceededCollateral = 7,
        CardOwnerNationalIdRequired = 8,
    }
}
