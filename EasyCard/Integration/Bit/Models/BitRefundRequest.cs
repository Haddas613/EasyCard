using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.Models
{
    public class BitRefundRequest
    {
        /// <summary>
        /// Required.
        /// </summary>
        public decimal CreditAmount { get; set; }

        /// <summary>
        /// Required.
        /// Requested currency (currently supporting only ILS) Example: ILS is 1
        /// </summary>
        public int CurrencyTypeCode { get; set; } = 1;

        /// <summary>
        /// Not required.
        /// Relates to the externalSystemReference of the payment initiation post request. If sent the validation is performed to ensure its value matched the paymentInitiationId.
        /// </summary>
        public string ExternalSystemReference { get; set; }

        public string RefundExternalSystemReference { get; set; }

        ///// <summary>
        ///// Required.
        ///// Franchising ID. This ID is used to determine the data related to franchising - such as terminalId no.
        ///// </summary>
        //public int FranchisingId { get; set; } = 176;

        ///// <summary>
        ///// Not required.
        ///// Provider Number. Used to indicate to which provider the payment relates. 
        ///// This field will be stored with the transaction data and sent to the clearing company on payment events such as capture and refund.
        ///// </summary>
        //public int ProviderNbr { get; set; }

        /// <summary>
        /// Required.
        /// ID of the payment initiation.
        /// </summary>
        public string PaymentInitiationId { get; set; }
    }
}
