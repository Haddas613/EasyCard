using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Transaction details
    /// </summary>
    [DataContract]
    public partial class TransactionSummary
    {
        /// <summary>
        /// Gets or Sets TransactionID
        /// </summary>
        [DataMember(Name = "transactionID")]
        public long? TransactionID { get; set; }

        /// <summary>
        /// Gets or Sets PaymentGateway
        /// </summary>
        [DataMember(Name = "paymentGateway")]
        public string PaymentGateway { get; set; }

        /// <summary>
        /// Gets or Sets TransactionDate
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
        /// Gets or Sets Currency
        /// </summary>
        [DataMember(Name = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Additional payments
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
        /// Gets or Sets TransactionDescription
        /// </summary>
        [DataMember(Name = "transactionDescription")]
        public string TransactionDescription { get; set; }

        /// <summary>
        /// Gets or Sets MerchantID
        /// </summary>
        [DataMember(Name = "merchantID")]
        public long? MerchantID { get; set; }

        /// <summary>
        /// Gets or Sets MerchantName
        /// </summary>
        [DataMember(Name = "merchantName")]
        public string MerchantName { get; set; }

        /// <summary>
        /// Gets or Sets MerchantReference
        /// </summary>
        [DataMember(Name = "merchantReference")]
        public string MerchantReference { get; set; }

        /// <summary>
        /// Gets or Sets BusinessArea
        /// </summary>
        [DataMember(Name = "businessArea")]
        public string BusinessArea { get; set; }

        /// <summary>
        /// Gets or Sets phone
        /// </summary>
        [DataMember(Name = "phone")]
        public string Phone { get; set; }

        /// <summary>
        /// Gets or Sets Business Id
        /// </summary>
        [DataMember(Name = "businessId")]
        public string BusinessId { get; set; }

        /// <summary>
        /// Gets or Sets ActivityStartDate
        /// </summary>
        [DataMember(Name = "activityStartDate")]
        public DateTime? ActivityStartDate { get; set; }

        /// <summary>
        /// Transaction type (regular, installments, credit)
        /// </summary>
        [DataMember(Name = "transactionType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionTypeEnum TransactionType { get; set; }
    }
}
