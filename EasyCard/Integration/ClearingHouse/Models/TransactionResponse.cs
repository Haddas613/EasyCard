using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Transaction Response
    /// </summary>
    [DataContract]
    public partial class TransactionResponse
    {
        /// <summary>
        /// Gets or Sets TransactionID
        /// </summary>
        [DataMember(Name = "transactionID")]
        public long? TransactionID { get; set; }

        /// <summary>
        /// Gets or Sets Merchant
        /// </summary>
        [DataMember(Name = "merchant")]
        public MerchantSummary Merchant { get; set; }

        /// <summary>
        /// Gets or Sets PaymentGateway
        /// </summary>
        [DataMember(Name = "paymentGateway")]
        public string PaymentGateway { get; set; }

        /// <summary>
        /// Date-time when initial transaction created in Clearing House
        /// </summary>
        [DataMember(Name = "transactionDate")]
        public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// Gets or Sets Status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionStatusEnum Status { get; set; }

        /// <summary>
        /// Gets or Sets PaymentGatewayTransactionDetails
        /// </summary>
        [DataMember(Name = "paymentGatewayTransactionDetails")]
        public PaymentGatewayTransactionDetails PaymentGatewayTransactionDetails { get; set; }

        /// <summary>
        /// Gets or Sets PaymentGatewayAdditionalDetails
        /// </summary>
        [DataMember(Name = "paymentGatewayAdditionalDetails")]
        public PaymentGatewayAdditionalDetails PaymentGatewayAdditionalDetails { get; set; }

        /// <summary>
        /// Gets or Sets Currency
        /// </summary>
        [DataMember(Name = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// One installment payment amount
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
        /// Payment amount including Clearing House commission
        /// </summary>
        [DataMember(Name = "totalAmount")]
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// Gets or Sets History
        /// </summary>
        [DataMember(Name = "history")]
        public List<TransactionHistory> History { get; set; }

        /// <summary>
        /// Check optimistic concurrency
        /// </summary>
        [DataMember(Name = "concurrencyToken")]
        public string ConcurrencyToken { get; set; }

        /// <summary>
        /// Tourist charges
        /// </summary>
        [DataMember(Name = "touristCharges")]
        public decimal? TouristCharges { get; set; }
    }
}
