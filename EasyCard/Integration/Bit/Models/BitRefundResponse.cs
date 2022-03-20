using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.Models
{
    public class BitRefundResponse : BitBaseResponse
    {
        public bool Success { get { return RequestStatusCode == "10"; } }

        public string RequestStatusCode { get; set; }

        public string RequestStatusDescription { get; set; }

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

    }
}
