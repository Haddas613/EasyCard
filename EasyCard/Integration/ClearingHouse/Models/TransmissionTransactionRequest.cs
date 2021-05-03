using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace ClearingHouse.Models
{
    public class TransmissionTransactionRequest
    {
        /// <summary>
        /// Payment Gateway ID in Clearing House system
        /// </summary>
        /// <value>Payment Gateway ID in Clearing House system</value>
        [DataMember(Name = "paymentGatewayID")]
        [Required(AllowEmptyStrings = false)]
        public int? PaymentGatewayID { get; set; }

        /// <summary>
        /// Transmission Date
        /// </summary>
        [DataMember(Name = "transmissionDate")]
        [Required(AllowEmptyStrings = false)]
        public DateTime? TransmissionDate { get; set; }

        /// <summary>
        /// List of transaction IDs
        /// </summary>
        [DataMember(Name = "transactionIDs")]
        public long[] TransactionIDs { get; set; }
    }
}
