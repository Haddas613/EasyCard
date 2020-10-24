using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum PaymentRequestStatusEnum : short
    {
        Initial = 0,

        /// <summary>
        /// Required because of technical reason - to do not send request twice
        /// </summary>
        Sending = 1,

        /// <summary>
        /// Request sent to user (pending user approval)
        /// </summary>
        Sent = 2,

        /// <summary>
        /// Can be re-sent
        /// </summary>
        SendingFailed = -1,

        /// <summary>
        /// Canceled by merchant
        /// </summary>
        Canceled = -2,

        /// <summary>
        /// Viewed by consumer
        /// </summary>
        Viewed = 3,

        /// <summary>
        /// Rejected by consumer
        /// </summary>
        Rejected = -3,

        // Note: we do not need this status - it will be calculated online
        // Overdue = -3,

        PaymentFailed = -4,

        Payed = 4
    }
}
