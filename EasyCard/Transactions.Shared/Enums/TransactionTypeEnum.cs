using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Enums
{
    /// <summary>
    /// 11 initialization/after initialization transaction
    /// 01 regular deal
    /// 53 refund
    /// </summary>
    public enum TransactionTypeEnum
    {
        Initialization,
        RegularDeal,
        Refund
    }
}
