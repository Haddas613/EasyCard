using System.Runtime.Serialization;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Create Transaction Request
    /// </summary>
    [DataContract]
    public partial class CreateTransactionRequest
    {
        /// <summary>
        /// Payment Gateway registered ID in Clearing House system
        /// </summary>
        [DataMember(Name = "paymentGatewayID")]
        public int? PaymentGatewayID { get; set; }

        /// <summary>
        /// Gets or Sets PaymentGatewayTransactionDetails
        /// </summary>
        [DataMember(Name = "paymentGatewayTransactionDetails")]
        public PaymentGatewayTransactionDetails PaymentGatewayTransactionDetails { get; set; }

        /// <summary>
        /// Gets or Sets Currency in 3 letters ISO format - like ILS, USD etc.
        /// </summary>
        [DataMember(Name = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Installments
        /// </summary>
        [DataMember(Name = "installmentPaymentAmount")]
        public decimal? InstallmentPaymentAmount { get; set; }

        /// <summary>
        /// Payment amount at merchant's side
        /// </summary>
        [DataMember(Name = "initialPaymentAmount")]
        public decimal? InitialPaymentAmount { get; set; }

        /// <summary>
        /// Number of payments
        /// </summary>
        [DataMember(Name = "payments")]
        public int? Payments { get; set; }

        /// <summary>
        /// Payment amount including Clearing House commission = InitialPaymentAmount + AdditionalPaymentsAmount
        /// </summary>
        [DataMember(Name = "totalAmount")]
        public decimal? TotalAmount { get; set; }

        [DataMember(Name = "cardNotPresent")]
        public bool? CardNotPresent { get; set; }
    }
}
