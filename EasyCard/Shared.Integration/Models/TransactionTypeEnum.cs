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
        Installments = 1,
        Credit = 2,
    }
}
