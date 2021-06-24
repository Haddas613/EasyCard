using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Upay.Models
{
    /// <summary>
    /// Commit transaction
    /// </summary>
    [DataContract]
    public partial class CommitTransactionRequest
    {
        /// <summary>
        ///  Gets or Sets Cashierid
        /// </summary>
        [DataMember(Name = "cashierid")]
        public string Cashierid { get; set; }

        /// <summary>
        /// Gets or Sets Sessionid
        /// </summary>
        [DataMember(Name = "sessionid")]
        public string Sessionid { get; set; }

        /// <summary>
        /// Gets or Sets Processor Code
        /// </summary>
        [DataMember(Name = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or Sets  Token
        /// </summary>
        [DataMember(Name = "token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or Sets  Foreigncard
        /// </summary>
        [DataMember(Name = "foreigncard")]
        public string Foreigncard { get; set; }

        /// <summary>
        /// Gets or Sets  Identitynum
        /// </summary>
        [DataMember(Name = "identitynum")]
        public string Identitynum { get; set; }

        /// <summary>
        /// Gets or Sets Cardcompany
        /// </summary>
        [DataMember(Name = "cardcompany")]
        public string Cardcompany { get; set; }

        /// <summary>
        /// Gets or Sets Cardtype
        /// </summary>
        [DataMember(Name = "cardtype")]
        public string Cardtype { get; set; }

        /// <summary>
        /// Gets or Sets Cardnumber
        /// </summary>
        [DataMember(Name = "cardnumber")]
        public string Cardnumber { get; set; }

        /// <summary>
        /// Gets or Sets Cardexpdate
        /// </summary>
        [DataMember(Name = "cardexpdate")]
        public string Cardexpdate { get; set; }

        /// <summary>
        /// Gets or Sets Paydate
        /// </summary>
        [DataMember(Name = "paydate")]
        public string Paydate { get; set; }

        /// <summary>
        /// Gets or Sets Payments
        /// </summary>
        [DataMember(Name = "payments")]
        public string Payments { get; set; }

        /// <summary>
        /// Gets or Sets Amount
        /// </summary>
        [DataMember(Name = "amount")]
        public string Amount { get; set; }

        public PaymentGatewayAdditionalDetails PaymentGatewayAdditionalDetails { get; set; }

        /// <summary>
        /// Gets or Sets Dealnumber
        /// </summary>
        [DataMember(Name = "dealnumber")]
        public string Dealnumber { get; set; }

        /// <summary>
        /// Gets or Sets Oknumber
        /// </summary>
        [DataMember(Name = "oknumber")]
        public string Oknumber { get; set; }

        /// <summary>
        /// Gets or Sets Cellphonenotify
        /// </summary>
        [DataMember(Name = "cellphonenotify")]
        public string Cellphonenotify { get; set; }

        /// <summary>
        /// Gets or Sets Sixdigits
        /// </summary>
        [DataMember(Name = "sixdigits")]
        public string Sixdigits { get; set; }
    }
}
