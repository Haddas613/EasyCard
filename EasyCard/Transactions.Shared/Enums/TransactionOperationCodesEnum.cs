using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum TransactionOperationCodesEnum : short
    {
        TransactionCreated,
        TransactionUpdated,
        TransactionRejected,
        TransactionCommitted,

        ChargebackCreated,
        ChargebackDone,
        ChargebackFailed,

        TranAddedToBillingReprt,
        TransactionTransferred,
        TransactionTransferFailed,

        ChrgbckAddedToBillingReprt,
        ChrgbckAddedToChrgbckReprt,

        ConfirmedByVisa,
        InstallmentConfirmedByVisa,

        RateRecalculation,
        UndoReject,

        Holded,
        Unholded,
        DepositedByVisa,

        ConfirmedByYazil,
        DepositedByYazil,
        ConvertToNikion
    }
}
