using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class BitTransactionDetails
    {
        /// <summary>
        /// Resource ID created by bit backend. ID represents the payment initiation. Used for Get /Delete, etc.
        /// </summary>
        public string BitPaymentInitiationId { get; set; }

        /// <summary>
        /// Additional UUID used for authentication. When using web client application this ID, along with paymentInitiationId, should be sent upon opening bit payment page (openBitPaymentPage).
        /// </summary>
        public string BitTransactionSerialId { get; set; }

        public string RequestStatusCode { get; set; }

        public string RequestStatusDescription { get; set; }

        /// <summary>
        /// Sub-terminal number
        /// </summary>
        public string BitMerchantNumber { get; set; }
    }
}
