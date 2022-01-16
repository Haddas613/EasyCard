using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.Models
{
    public enum BitRequestStatusCodeEnum
    {
        /// <summary>
        /// The application was proactively canceled by the business or due to a failure in the process.
        /// </summary>
        FinalCanceled = 2,

        /// <summary>
        /// The client stopped the process before credit extension confirmed .
        /// </summary>
        FinalDenied = 3,

        /// <summary>
        /// Interim status between receiving an instruction and performing an action on the transaction until the end of that action (when extending credit, charging a fee and performing a refund).
        /// </summary>
        RequestPending = 4,

        /// <summary>
        /// Money cannot be transferred by bit.
        /// </summary>
        CanceledByBit = 5,

        /// <summary>
        /// Request expired (expiration time until execution J5).
        /// </summary>
        Expired = 7,

        /// <summary>
        /// Credit extension performed. Awaiting instruction to make payment by the business.
        /// </summary>
        CreditExtensionPerformed = 9,

        /// <summary>
        /// Partial or full refund was made.
        /// </summary>
        Refunded = 10,

        /// <summary>
        /// Credit card charged .
        /// </summary>
        Charged = 11,

        /// <summary>
        /// A request for payment was established by the business.
        /// </summary>
        RequestWasMade = 12,

        /// <summary>
        /// No credit card (TO) response was received. The system will interrogate until a final answer is obtained.
        /// </summary>
        ConfirmingInProcess = 13,

        /// <summary>
        /// No credit card response (TO) was received. The system will interrogate until a final answer is obtained .
        /// </summary>
        ConfirmingRefundInProcess = 14,

        /// <summary>
        /// Debit confirmation ended unanswered. The transaction was transferred to manual billing.
        /// </summary>
        TransferredToManualBilling = 15,

        /// <summary>
        /// Refund confirmation ended unanswered. The transaction was transferred to manual handling.
        /// </summary>
        TransferredToManualRefund = 16,

        /// <summary>
        /// Customer has not yet joined the transaction (did not scan the QR / did not enter the SMS link).
        /// </summary>
        WaitingForCustomerToJoin = 17
    }
}
