using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Upay.Models
{
    public class TransfersModel: ParameterBase
    {
        /// <summary>
        /// Gets or Sets email
        /// </summary>
        [DataMember(Name = "email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or Sets amount
        /// </summary>
        [DataMember(Name = "amount")]
        public string Amount { get; set; }

        /// <summary>
        /// Gets or Sets newvalue
        /// </summary>
        [DataMember(Name = "newvalue")]
        public string Newvalue { get; set; }

        /// <summary>
        /// Gets or Sets numberpayments
        /// </summary>
        [DataMember(Name = "numberpayments")]
        public string Numberpayments { get; set; }

        /// <summary>
        /// Gets or Sets currency
        /// </summary>
        [DataMember(Name = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Gets or Sets commissionreduction
        /// </summary>
        [DataMember(Name = "commissionreduction")]
        public string Commissionreduction { get; set; }

        /// <summary>
        /// Gets or Sets acceptedtransaction
        /// </summary>
        [DataMember(Name = "acceptedtransaction")]
        public string Acceptedtransaction { get; set; }

        /// <summary>
        /// Gets or Sets paymentdate
        /// </summary>
        [DataMember(Name = "paymentdate")]
        public string Paymentdate { get; set; }

        /// <summary>
        /// Gets or Sets comment
        /// </summary>
        [DataMember(Name = "comment")]
        public string Comment { get; set; }

        /// <summary>
        /// Gets or Sets cellphonenotify
        /// </summary>
        [DataMember(Name = "cellphonenotify")]
        public string Cellphonenotify { get; set; }

        /// <summary>
        /// Gets or Sets token
        /// </summary>
        [DataMember(Name = "token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or Sets ipnurl
        /// </summary>
        [DataMember(Name = "ipnurl")]
        public string Ipnurl { get; set; }

        /// <summary>
        /// Gets or Sets returnurl
        /// </summary>
        [DataMember(Name = "returnurl")]
        public string Returnurl { get; set; }
    }
}
