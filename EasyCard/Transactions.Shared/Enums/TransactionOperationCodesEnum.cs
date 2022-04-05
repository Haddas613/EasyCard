using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum TransactionOperationCodesEnum : short
    {
        TransactionCreated = 0,

        TransactionUpdated = 1,

        ConfirmedByAggregator = 2,

        ConfirmedByProcessor = 3,

        CommitedByAggregator = 4,

        TransmissionInProgress = 5,

        TransmissionCancelingInProgress = 6,

        TransmittedByProcessor = 7,

        RejectedByAggregator = 8,

        RejectedByProcessor = 9,

        CancelledByMerchant = 10,

        FailedToConfirmByAggregator = 11,

        FailedToConfirmByProcesor = 12,

        FailedToCommitByAggregator = 13,

        TransmissionToProcessorFailed = 14,

        FailedToCancelByAggregator = 15,

        CanceledByAggregator = 16,

        InvoiceCreated = 17,

        ProcessorPreTransmissionCommited = 18,

        RefundCreated = 19,

        RefundFailed = 20
    }
}
