using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum QuickTypeEnum
    {
        /// <summary>
        /// Any other tran type and status
        /// </summary>
        Other = 1,

        /// <summary>
        /// Regular J4 successfull transaction (can be not-transmitted, can be installment or credit), finalization status can be failed
        /// </summary>
        Regular = 0,

        /// <summary>
        /// Regular successfull refund
        /// </summary>
        Refund = -1,
    }
}
