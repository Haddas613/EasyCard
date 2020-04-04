using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum TransactionStatusEnum : short
    {
        Initial = 0,

        /// <summary>
        /// Clearing House confirmed transaction
        /// </summary>
        ConfirmedByAggregator = 10,

        /// <summary>
        /// Shva confirmed transaction
        /// </summary>
        ConfirmedByProcessor = 20,

        /// <summary>
        /// Transaction commited to Clearing House
        /// </summary>
        CommitedToAggregator = 30,

        /// <summary>
        /// Shva transmision is done
        /// </summary>
        TransmittedToProcessor = 40,

        /// <summary>
        /// Transaction is rejeced by Clearing House
        /// </summary>
        RejectedByAggregator = -10,

        /// <summary>
        /// Not possible to confirm by Clearing House
        /// </summary>
        FailedToConfirmByAggregator  = -20,

        /// <summary>
        /// Failed to commit by Clearing House
        /// </summary>
        FailedToCommitByAggregator = -30,

        /// <summary>
        /// Transaction is rejected by Shva
        /// </summary>
        RejectedByProcessor = -20,

        /// <summary>
        /// Merchant cancelled transaction before transmitting to Shva
        /// </summary>
        CancelledByMerchant = -30,

        /// <summary>
        /// Transaction cancellation is sent to Clearing House
        /// </summary>
        RejectedToAggregator = -30,
        
        /// <summary>
        /// Shva transmission failed
        /// </summary>
        TransmissionToProcessorFailed = -40

    }
}
