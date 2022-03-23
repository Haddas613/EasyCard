using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Bit.Models
{
    public class BitRequestStatusCodeResult
    {
        public BitRequestStatusCodeResult(BitRequestStatusCodeEnum bitRequestStatusCode, bool final, bool failed)
        {
            BitRequestStatusCode = bitRequestStatusCode;
            Final = final;
            Failed = failed;
        }

        static BitRequestStatusCodeResult()
        {
            var allCodesList = new List<BitRequestStatusCodeResult>
            {
                new BitRequestStatusCodeResult( BitRequestStatusCodeEnum.FinalCanceled,  true, true),
                new BitRequestStatusCodeResult( BitRequestStatusCodeEnum.FinalDenied,  true, true),
                new BitRequestStatusCodeResult( BitRequestStatusCodeEnum.RequestPending,  false, false),
                new BitRequestStatusCodeResult( BitRequestStatusCodeEnum.CanceledByBit,  true, true),
                new BitRequestStatusCodeResult( BitRequestStatusCodeEnum.Expired,  true, true),
                new BitRequestStatusCodeResult( BitRequestStatusCodeEnum.CreditExtensionPerformed,  false, false),
                new BitRequestStatusCodeResult( BitRequestStatusCodeEnum.Refunded,  true, false),
                new BitRequestStatusCodeResult( BitRequestStatusCodeEnum.Charged,  true, false),
                new BitRequestStatusCodeResult( BitRequestStatusCodeEnum.RequestWasMade,  false, false),
                new BitRequestStatusCodeResult( BitRequestStatusCodeEnum.ConfirmingInProcess,  false, false),
                new BitRequestStatusCodeResult( BitRequestStatusCodeEnum.ConfirmingRefundInProcess,  false, false),
                new BitRequestStatusCodeResult( BitRequestStatusCodeEnum.TransferredToManualBilling,  true, true), // ?
                new BitRequestStatusCodeResult( BitRequestStatusCodeEnum.TransferredToManualRefund,  true, true), // ?
                new BitRequestStatusCodeResult( BitRequestStatusCodeEnum.WaitingForCustomerToJoin,  false, false),
            };

            allCodes = new ReadOnlyDictionary<BitRequestStatusCodeEnum, BitRequestStatusCodeResult>(allCodesList.ToDictionary(d => d.BitRequestStatusCode));
        }

        public static BitRequestStatusCodeResult ParseResult(string resultCodeStr)
        {
            if (!int.TryParse(resultCodeStr, out var resultCode))
            {
                return null;
            }

            if (!Enum.IsDefined(typeof(BitRequestStatusCodeEnum), resultCode))
            {
                return null;
            }

            if (!allCodes.TryGetValue((BitRequestStatusCodeEnum)resultCode, out var res))
            {
                return null;
            }

            return res;
        }

        public BitRequestStatusCodeEnum BitRequestStatusCode { get; set; }

        public bool Final { get; set; }

        public bool Failed { get; set; }

        private static readonly IReadOnlyDictionary<BitRequestStatusCodeEnum, BitRequestStatusCodeResult> allCodes;

        public static IReadOnlyDictionary<BitRequestStatusCodeEnum, BitRequestStatusCodeResult> AllCodes { get { return allCodes; } }
    }

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
