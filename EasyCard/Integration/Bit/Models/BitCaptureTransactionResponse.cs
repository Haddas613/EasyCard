using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.Models
{
    [Obsolete("Remove if not needed")]
    public class BitCaptureTransactionResponse : ProcessorCreateTransactionResponse
    {
        public decimal RequestAmount { get; set; }
        
        /// <summary>
        /// Requested currency (currently supporting only ILS) Example: ILS is 1
        /// </summary>
        public int CurrencyTypeCode { get; set; } = 1;

        /// <summary>
        /// Relates to the externalSystemReference of the payment initiation post request. If sent the validation is performed to ensure its value matched the paymentInitiationId.
        /// </summary>
        public string ExternalSystemReference { get; set; }

        /// <summary>
        /// ID of the payment initiation.
        /// </summary>
        public string PaymentInitiationId { get; set; }

        /// <summary>
        /// Usually referred to as the X field in the Israeli card industry. Serves as a reference ID for supporting inquiries.
        /// </summary>
        public long SourceTransactionId { get; set; }

        /// <summary>
        /// Usually referred to as the Z field in the Israeli card industry. Serves as a reference ID for supporting inquiries.
        /// </summary>
        public string IssuerTransactionId { get; set; }

        /// <summary>
        /// Issuer Authorization number as returned from the card issuer if capture is approved.
        /// </summary>
        public long IssuerAuthorizationNumber { get; set; }

        /// <summary>
        /// Last 4 digits of the paying credit card.
        /// </summary>
        public string SuffixPlasticCardNumber { get; set; }
    }
}
