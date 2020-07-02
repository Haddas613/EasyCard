using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum TransactionStatusEnum : short
    {
        [EnumMember(Value = "initial")]
        Initial = 0,

        /// <summary>
        /// Clearing House confirmed transaction
        /// </summary>
        [EnumMember(Value = "confirmedByAggregator")]
        ConfirmedByAggregator = 10,

        /// <summary>
        /// Shva confirmed transaction
        /// </summary>
        [EnumMember(Value = "confirmedByProcessor")]
        ConfirmedByProcessor = 20,

        /// <summary>
        /// Transaction commited to Clearing House
        /// </summary>
        [EnumMember(Value = "commitedByAggregator")]
        CommitedByAggregator = 30,

        /// <summary>
        /// Shva transmision in progress
        /// </summary>
        [EnumMember(Value = "transmissionInProgress")]
        TransmissionInProgress = 35,

        /// <summary>
        /// Canceling transmision in progress
        /// </summary>
        [EnumMember(Value = "transmissionCancelingInProgress")]
        TransmissionCancelingInProgress = 36,

        /// <summary>
        /// Shva transmision is done
        /// </summary>
        [EnumMember(Value = "transmittedByProcessor")]
        TransmittedByProcessor = 40,

        /// <summary>
        /// Transaction is rejeced by Clearing House
        /// </summary>
        [EnumMember(Value = "rejectedByAggregator")]
        RejectedByAggregator = -10,

        /// <summary>
        /// Transaction is rejected by Shva
        /// </summary>
        [EnumMember(Value = "rejectedByProcessor")]
        RejectedByProcessor = -20,

        /// <summary>
        /// Merchant cancelled transaction before transmitting to Shva
        /// </summary>
        [EnumMember(Value = "cancelledByMerchant")]
        CancelledByMerchant = -30,

        /// <summary>
        /// Not possible to confirm by Clearing House
        /// </summary>
        [EnumMember(Value = "failedToConfirmByAggregator")]
        FailedToConfirmByAggregator  = -50,

        /// <summary>
        /// Not possible to confirm by Shva
        /// </summary>
        [EnumMember(Value = "failedToConfirmByProcesor")]
        FailedToConfirmByProcesor = -60,

        /// <summary>
        /// Failed to commit by Clearing House
        /// </summary>
        [EnumMember(Value = "failedToCommitByAggregator")]
        FailedToCommitByAggregator = -70,

        /// <summary>
        /// Shva transmission failed
        /// </summary>
        [EnumMember(Value = "transmissionToProcessorFailed")]
        TransmissionToProcessorFailed = -80,
    }
}
