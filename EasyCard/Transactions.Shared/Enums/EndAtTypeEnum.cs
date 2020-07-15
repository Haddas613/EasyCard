using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum EndAtTypeEnum
    {
        Never = 0,
        SpecifiedDate = 1,
        AfterNumberOfPayments = 2
    }
}
