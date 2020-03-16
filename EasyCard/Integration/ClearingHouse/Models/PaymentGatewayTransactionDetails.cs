using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Transaction details in Payment Gateway system
    /// </summary>
    [DataContract]
    public partial class PaymentGatewayTransactionDetails
    {
        /// <summary>
        /// Date-time when initial transaction created in Payment Gateway system
        /// </summary>
        [DataMember(Name = "transactionDate")]
        public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// Payment Gateway Deal ID (external reference)
        /// </summary>
        [DataMember(Name = "dealReference")]
        public string DealReference { get; set; }

        /// <summary>
        /// Terminal ID in Shva
        /// </summary>
        [DataMember(Name = "terminalReference")]
        public string TerminalReference { get; set; }

        /// <summary>
        /// Merchant&#39;s Token
        /// </summary>
        [DataMember(Name = "merchantReference")]
        public string MerchantReference { get; set; }

        /// <summary>
        /// Gets or Sets DealDescription
        /// </summary>
        [DataMember(Name = "dealDescription")]
        public string DealDescription { get; set; }

        /// <summary>
        /// Gets or Sets CreditCardVendor
        /// </summary>
        [DataMember(Name = "creditCardVendor")]
        public string CreditCardVendor { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        [DataMember(Name = "consumerEmail")]
        public string ConsumerEmail { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        [DataMember(Name = "consumerPhone")]
        public string ConsumerPhone { get; set; }

        /// <summary>
        /// Consumer name
        /// </summary>
        [DataMember(Name = "cardOwnerName")]
        public string CardOwnerName { get; set; }

        /// <summary>
        /// Credit Card Last Four Digits
        /// </summary>
        [DataMember(Name = "cardLastFourDigits")]
        public string CardLastFourDigits { get; set; }

        /// <summary>
        /// Credit Card Expiration in format MMyy
        /// </summary>
        [DataMember(Name = "cardExpiration")]
        public string CardExpiration { get; set; }

        /// <summary>
        /// Clearing company
        /// </summary>
        [DataMember(Name = "solek")]
        public int? Solek { get; set; }

        /// <summary>
        /// Transaction type (regular, installments, credit)
        /// </summary>
        [DataMember(Name = "transactionType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionTypeEnum TransactionType { get; set; }

        /// <summary>
        /// Credit Card First Six (eight) Digits
        /// </summary>
        [DataMember(Name = "cardBin")]
        public string CardBin { get; set; }

        /// <summary>
        /// Credit Card Last Four and First Six Digits - XXXXXX****XXXX
        /// </summary>
        [DataMember(Name = "cardDigits")]
        public string CardDigits { get; set; }

        /// <summary>
        /// Credit Card owner national ID
        /// </summary>
        [DataMember(Name = "cardOwnerNationalId")]
        public string CardOwnerNationalId { get; set; }
    }
}
