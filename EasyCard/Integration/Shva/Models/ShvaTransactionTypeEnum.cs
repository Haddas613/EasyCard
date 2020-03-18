using System;
using System.Collections.Generic;
using System.Text;

namespace Shva.Models
{
    /// <summary>
    /// 11 initialization/after initialization transaction
    /// 01 regular deal
    /// 53 refund
    /// </summary>
    public enum ShvaTransactionTypeEnum
    {
        RegularDeal = 01,
        FirstInstallment = 11, // TODO: is it first installment or any installment ?
        Refund = 53,
    }
}
