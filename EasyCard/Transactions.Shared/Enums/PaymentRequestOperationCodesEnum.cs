using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum PaymentRequestOperationCodesEnum : short
    {
        PaymentRequestCreated,

        PaymentRequestUpdated,

        PaymentRequestSent,

        PaymentRequestCanceled,

        PaymentRequestViewed,

        PaymentRequestRejected,

        PaymentRequestPaymentFailed,

        PaymentRequestPayed,
    }
}
