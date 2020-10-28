using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum PaymentRequestStatusEnum : short
    {
        [EnumMember(Value = "initial")]
        Initial = 0,

        /// <summary>
        /// Required because of technical reason - to do not send request twice
        /// </summary>
        [EnumMember(Value = "sending")]
        Sending = 1,

        /// <summary>
        /// Request sent to user (pending user approval)
        /// </summary>
        [EnumMember(Value = "sent")]
        Sent = 2,

        /// <summary>
        /// Can be re-sent
        /// </summary>
        [EnumMember(Value = "sendingFailed")]
        SendingFailed = -1,

        /// <summary>
        /// Canceled by merchant
        /// </summary>
        [EnumMember(Value = "canceled")]
        Canceled = -2,

        /// <summary>
        /// Viewed by consumer
        /// </summary>
        [EnumMember(Value = "viewed")]
        Viewed = 3,

        /// <summary>
        /// Rejected by consumer
        /// </summary>
        [EnumMember(Value = "rejected")]
        Rejected = -3,

        // Note: we do not need this status - it will be calculated online
        // Overdue = -3,
        [EnumMember(Value = "paymentFailed")]
        PaymentFailed = -4,

        [EnumMember(Value = "payed")]
        Payed = 4
    }
}
