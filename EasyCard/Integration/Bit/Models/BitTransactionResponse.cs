using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.Models
{
    public class BitTransactionResponse
    {
        public decimal RequestAmount { get; set; }

        /// <summary>
        /// Requested currency (currently supporting only ILS) Example: ILS is 1
        /// </summary>
        public int CurrencyTypeCode { get; set; }

        /// <summary>
        /// 1 = Immediate payment (J4 for credit card industry) 2 = Hold funds(J5 in credit card industry)
        /// </summary>
        public int DebitMethodCode { get; set; }

        /// <summary>
        /// Logical payment instruction ID. If a Post request is repeated it should hold the same endToEndIdentification (idempotency key)
        /// </summary>
        public string ExternalSystemReference { get; set; }

        /// <summary>
        /// Free text describing payment
        /// </summary>
        public string RequestSubjectDescription { get; set; }

        /// <summary>
        /// Franchising ID. This ID is used to determine the data related to franchising - such as terminalId no.
        /// </summary>
        public int? FranchisingId { get; set; }

        public string ApplicationSchemeAndroid { get; set; }
        public string ApplicationSchemeIos { get; set; }

        /// <summary>
        /// Expiration date time of payment initiation. If funds are not held the payment initiation will be expired according to the eventExpirationTimestamp.
        /// </summary>
        public string EventExpirationTimestamp { get; set; }

        /// <summary>
        /// Link to the bit app in Google Play or Apple store.
        /// </summary>
        public string LinkAddress { get; set; }

        /// <summary>
        /// Resource ID created by bit backend. ID represents the payment initiation. Used for Get /Delete, etc.
        /// </summary>
        public string PaymentInitiationId { get; set; }

        /// <summary>
        /// Additional UUID used for authentication. When using web client application this ID, along with paymentInitiationId, should be sent upon opening bit payment page (openBitPaymentPage).
        /// </summary>
        public string TransactionSerialId { get; set; }

        /// <summary>
        /// This is the payment initiation status code.
        /// received - 1,
        /// rejected - 2,
        /// partiallyAuthorised - 3,
        /// valid - 4,
        /// revokedByPsu - 5,
        /// expired - 6,
        /// terminatedByTpp - 7,
        /// charged - 11
        /// </summary>
        public string RequestStatusCode { get; set; }

        public BitRequestStatusCodeResult RequestStatusCodeResult
        {
            get
            {
                return BitRequestStatusCodeResult.ParseResult(RequestStatusCode);
            }
        }

        public bool Success
        {
            get
            {
                return RequestStatusCodeResult?.Final == true && RequestStatusCodeResult?.Failed == false;
            }
        }

        /// <summary>
        /// This is the payment initiation status code description.
        /// </summary>
        public string RequestStatusDescription { get; set; }

        /// <summary>
        /// Usually referred to as the X field in the Israeli card industry. Serves as a reference ID for supporting inquiries.
        /// </summary>
        public long SourceTransactionId { get; set; }

        /// <summary>
        /// Issuer Authorization number, as returned from the card issuer if capture is approved.
        /// </summary>
        public long IssuerAuthorizationNumber { get; set; }

        /// <summary>
        /// Provider Number. Used to indicate to which provider the payment relates. Sent only if this field was in the input of the POST initiate payment request.
        /// </summary>
        public int ProviderNbr { get; set; }

        /// <summary>
        /// URL to bit web page.
        /// </summary>
        public string PaymentPageUrlAddress { get; set; }

        /// <summary>
        /// Usually referred to as the Z field in the Israeli card industry. Serves as a reference ID for supporting inquiries.
        /// </summary>
        public string IssuerTransactionId { get; set; }
    }
}
