using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.Models
{
    public class BitCreateTransactionResponse : ProcessorCreateTransactionResponse
    {
        public decimal RequestAmount { get; set; }

        /// <summary>
        /// Requested currency (currently supporting only ILS) Example: ILS is 1
        /// </summary>
        public int CurrencyTypeCode { get; set; } = 1;

        /// <summary>
        /// Only value 1 (J4) is supported.
        /// </summary>
        public int DebitMethodCode { get; set; } = 1;

        /// <summary>
        /// Relates to the externalSystemReference of the payment initiation post request. If sent the validation is performed to ensure its value matched the paymentInitiationId.
        /// </summary>
        public string ExternalSystemReference { get; set; }

        /// <summary>
        /// Free text describing payment purpose: max length 50
        /// </summary>
        public string RequestSubjectDescription { get; set; }

        /// <summary>
        /// Franchising ID. This ID is used to determine the data related to franchising - such as terminalId no.
        /// </summary>
        public int FranchisingId { get; set; }

        /// <summary>
        /// Provider Number. Used to indicate to which provider the payment relates. 
        /// This field will be stored with the transaction data and sent to the clearing company on payment events such as capture and refund.
        /// </summary>
        public int ProviderNbr { get; set; }

        /// <summary>
        /// Return Address to your client. In order to implement a responsive browser on mobile, you need to send this URL.
        /// </summary>
        public string UrlReturnAddress { get; set; }

        /// <summary>
        /// Used by Android merchant app to open the bit app.
        /// </summary>
        public string ApplicationSchemeAndroid { get; set; }

        /// <summary>
        /// Used by iOS merchant app to open the bit app.
        /// </summary>
        public string ApplicationSchemeIos { get; set; }

        /// <summary>
        /// Link to bit app in Google Play or Apple store.
        /// </summary>
        public string LinkAddresss { get; set; }

        /// <summary>
        /// Resource ID created by bit backend. Represents the payment initiation resource ID. 
        /// The ID is used for other resource endpoints, such as Get /Delete, etc. 
        /// This ID, along with the transactionSerialId (see below), should be sent upon opening the bit payment page (openBitPaymentPage).
        /// </summary>
        public string PaymentInitiationId { get; set; }

        /// <summary>
        /// Additional UUID used for authentication. When using a web client application,
        /// this ID along with the paymentInitiationId (see above), should be sent upon opening the bit payment page (openBitPaymentPage).
        /// </summary>
        public string TransactionSerialId { get; set; }

        /// <summary>
        /// URL address to open bit web page
        /// </summary>
        public string PaymentPageUrlAddress { get; set; }
    }
}
