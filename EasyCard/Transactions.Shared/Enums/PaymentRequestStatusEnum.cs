using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum PaymentRequestStatusEnum : short
    {
        Initial = 0,

        Sending = 1,

        /// <summary>
        /// Request sent to user (pending user approval)
        /// </summary>
        Sent = 2,

        SendingFailed = -1,

        Viewed = 3,

        Rejected = -2,

        Overdue = -3,

        PaymentFailed = -4,

        Payd = 4
    }
}
