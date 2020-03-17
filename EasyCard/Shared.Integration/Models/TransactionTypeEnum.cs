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
        FirstInstallment = 1, // TODO: is it first installment or any installment ?
        Credit = 2,
        Refund = 3,
        J5Block = 4,
        Check = 5 // see Shva ParamJEnum
    }
}
