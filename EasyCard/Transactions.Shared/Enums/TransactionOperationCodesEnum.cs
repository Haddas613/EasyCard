using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum TransactionOperationCodesEnum : short
    {
        TransactionCreated,

        TransactionUpdated,

        ConfirmedByAggregator,

        ConfirmedByProcessor,

        CommitedByAggregator,

        TransmissionInProgress,

        TransmissionCancelingInProgress,

        TransmittedByProcessor,

        RejectedByAggregator,

        RejectedByProcessor,

        CancelledByMerchant,

        FailedToConfirmByAggregator,

        FailedToConfirmByProcesor,

        FailedToCommitByAggregator,

        TransmissionToProcessorFailed,

        FailedToCancelByAggregator,

        CanceledByAggregator,

        InvoiceCreated,

        ProcessorPreTransmissionCommited
    }
}
