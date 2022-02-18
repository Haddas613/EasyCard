using System.Runtime.Serialization;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Commit transaction
    /// </summary>
    [DataContract]
    public partial class CommitTransactionRequest
    {
        /// <summary>
        /// Payment Gateway ID in Clearing House system
        /// </summary>
        [DataMember(Name = "paymentGatewayID")]
        public int? PaymentGatewayID { get; set; }

        /// <summary>
        /// Gets or Sets PaymentGatewayAdditionalDetails
        /// </summary>
        [DataMember(Name = "paymentGatewayAdditionalDetails")]
        public PaymentGatewayAdditionalDetails PaymentGatewayAdditionalDetails { get; set; }

        /// <summary>
        /// Check optimistic concurrency
        /// </summary>
        [DataMember(Name = "concurrencyToken")]
        public string ConcurrencyToken { get; set; }

        /// <summary>
        /// Clearing company
        /// </summary>
        [DataMember(Name = "solek")]
        public int? Solek { get; set; }

        /// <summary>
        /// Gets or Sets CreditCardVendor
        /// </summary>
        [DataMember(Name = "creditCardVendor")]
        public string CreditCardVendor { get; set; }

        [DataMember(Name = "dealReference")]
        public string DealReference { get; set; }

        /// <summary>
        /// Tourist charges
        /// </summary>
        [DataMember(Name = "isTourist")]
        public bool? IsTourist { get; set; }

        /// <summary>
        /// Credit Card First Six (eight) Digits
        /// </summary>
        [DataMember(Name = "cardBin")]
        public string CardBin { get; set; }

        /// <summary>
        /// Credit Card Last Four Digits
        /// </summary>
        [DataMember(Name = "cardLastFourDigits")]
        public string CardLastFourDigits { get; set; }

        /// <summary>
        /// Is Bit Transaction
        /// </summary>
        [DataMember(Name = "isBit")]
        public bool IsBit { get; set; }
    }
}
