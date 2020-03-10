using System.Runtime.Serialization;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Commit transaction
    /// </summary>
    [DataContract]
    public partial class RejectTransactionRequest
    {
        /// <summary>
        /// Payment Gateway ID in Clearing House system
        /// </summary>
        /// <value>Payment Gateway ID in Clearing House system</value>
        [DataMember(Name = "paymentGatewayID")]
        public int? PaymentGatewayID { get; set; }

        /// <summary>
        /// Check optimistic concurrency
        /// </summary>
        [DataMember(Name = "concurrencyToken")]
        public string ConcurrencyToken { get; set; }

        /// <summary>
        /// Rejection Reason
        /// </summary>
        [DataMember(Name = "rejectionReason")]
        public string RejectionReason { get; set; }
    }
}
