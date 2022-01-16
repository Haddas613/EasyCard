using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.Models
{
    public class BitCaptureRequest
    {
        /// <summary>
        /// Required.
        /// </summary>
        public decimal RequestAmount { get; set; }

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

        /// <summary>
        /// Required.
        /// ID of the payment initiation.
        /// </summary>
        public string PaymentInitiationId { get; set; }

        /// <summary>
        /// Required.
        /// Usually referred to as the X field in the Israeli card industry. Serves as a reference ID for supporting inquiries.
        /// </summary>
        public long SourceTransactionId { get; set; }

        /// <summary>
        /// Not required.
        /// Usually referred to as the Z field in the Israeli card industry. Serves as a reference ID for supporting inquiries.
        /// </summary>
        public string IssuerTransactionId { get; set; }
    }
}
