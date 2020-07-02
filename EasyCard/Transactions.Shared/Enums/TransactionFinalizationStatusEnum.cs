using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum TransactionFinalizationStatusEnum : short
    {
        [EnumMember(Value = "initial")]
        Initial = 0,

        /// <summary>
        /// Failed to cancel transaction by Clearing House
        /// </summary>
        [EnumMember(Value = "failedToCancelByAggregator")]
        FailedToCancelByAggregator = -90,

        /// <summary>
        /// Transaction cancellation is done to Clearing House
        /// </summary>
        [EnumMember(Value = "canceledByAggregator")]
        CanceledByAggregator = -40,
    }
}
