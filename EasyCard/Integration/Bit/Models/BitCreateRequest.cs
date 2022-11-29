using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Shared.Api.Models.Binding;

namespace Bit.Models
{
    public class BitCreateRequest
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
        /// Only value 2 (J5) is supported. Required
        /// </summary>
        public int DebitMethodCode { get; set; } = 2;

        /// <summary>
        /// Not required.
        /// Relates to the externalSystemReference of the payment initiation post request. If sent the validation is performed to ensure its value matched the paymentInitiationId.
        /// </summary>
        public string ExternalSystemReference { get; set; }

        /// <summary>
        /// Required.
        /// Free text describing payment purpose: max length 50
        /// </summary>
        [JsonConverter(typeof(EscapingHtmlConverter))]
        public string RequestSubjectDescription { get; set; }

        /// <summary>
        /// Required.
        /// Franchising ID. This ID is used to determine the data related to franchising - such as terminalId no.
        /// </summary>
        public int FranchisingId { get; set; }

        /// <summary>
        /// Not required.
        /// Provider Number. Used to indicate to which provider the payment relates. 
        /// This field will be stored with the transaction data and sent to the clearing company on payment events such as capture and refund.
        /// </summary>
        public int ProviderNbr { get; set; }

        /// <summary>
        /// Not required.
        /// Return Address to your client. In order to implement a responsive browser on mobile, you need to send this URL.
        /// </summary>
        public string UrlReturnAddress { get; set; }
    }
}
