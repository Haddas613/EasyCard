using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    /// <summary>
    /// TransactionTypeEnum
    /// </summary>
    public enum TransactionTypeEnum : short
    {
        RegularDeal = 0,
        FirstInstallment = 1,
        Credit = 2,
        Refund = 3,
        J5Block = 4
    }
}
