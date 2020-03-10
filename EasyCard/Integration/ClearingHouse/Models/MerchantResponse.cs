using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Merchant object
    /// </summary>
    [DataContract]
    public partial class MerchantResponse
    {
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
        /// Gets or Sets ActivityStartDate
        /// </summary>
        [DataMember(Name = "activityStartDate")]
        public DateTime? ActivityStartDate { get; set; }

        /// <summary>
        /// Merchant&#39;s Token
        /// </summary>
        [DataMember(Name = "merchantReference")]
        public string MerchantReference { get; set; }

        /// <summary>
        /// Gets or Sets RiskRate
        /// </summary>
        [DataMember(Name = "riskRate")]
        public int? RiskRate { get; set; }

        /// <summary>
        /// KYC approval status e.g. pending, approved, rejected
        /// </summary>
        [DataMember(Name = "kycApprovalStatus")]
        [JsonConverter(typeof(StringEnumConverter))]
        public KYCStatusEnum? KycApprovalStatus { get; set; }

        /// <summary>
        /// KYS notes
        /// </summary>
        [DataMember(Name = "kycNotes")]
        public string KycNotes { get; set; }

        /// <summary>
        /// Gets or Sets BusinessDetails
        /// </summary>
        [DataMember(Name = "businessDetails")]
        public BusinessDetails BusinessDetails { get; set; }

        /// <summary>
        /// Gets or Sets PaymentGatewayDetails
        /// </summary>
        [DataMember(Name = "paymentGatewayDetails")]
        public PaymentGatewayMerchantDetails PaymentGatewayDetails { get; set; }

        /// <summary>
        /// Gets or Sets PersonalDetails
        /// </summary>
        [DataMember(Name = "personalDetails")]
        public PersonalDetails PersonalDetails { get; set; }

        /// <summary>
        /// Gets or Sets BankAccount
        /// </summary>
        [DataMember(Name = "bankAccount")]
        public BankAccount BankAccount { get; set; }

        /// <summary>
        /// Gets or Sets CreditCardAccount
        /// </summary>
        [DataMember(Name = "creditCardAccount")]
        public CreditCardAccount CreditCardAccount { get; set; }

        /// <summary>
        /// Current value from CommissionRates
        /// </summary>
        [DataMember(Name = "currentBasicComissionRate")]
        public decimal? CurrentBasicComissionRate { get; set; }

        /// <summary>
        /// Current value from CommissionRates
        /// </summary>
        [DataMember(Name = "currentInstallmentComissionRate")]
        public decimal? CurrentInstallmentComissionRate { get; set; }

        /// <summary>
        /// Current value from SecurityLimits
        /// </summary>
        [DataMember(Name = "currentCollateralSecurityLimit")]
        public decimal? CurrentCollateralSecurityLimit { get; set; }

        /// <summary>
        /// Current value from SecurityLimits
        /// </summary>
        [DataMember(Name = "currentCreditCardJ5Blocks")]
        public decimal? CurrentCreditCardJ5Blocks { get; set; }

        /// <summary>
        /// Check optimistic concurrency
        /// </summary>
        [DataMember(Name = "concurrencyToken")]
        public string ConcurrencyToken
        {
            get
            {
                return Timestamp == null ? null : Convert.ToBase64String(Timestamp);
            }

            set
            {
                if (value == null)
                {
                    Timestamp = null;
                }
                else
                {
                    Timestamp = Convert.FromBase64String(value);
                }
            }
        }

        [JsonIgnore]
        public byte[] Timestamp { get; set; }

        /// <summary>
        /// User successfully registered at Identity server
        /// </summary>
        [DataMember(Name = "identityEstablished")]
        public bool IdentityEstablished { get; set; }

        /// <summary>
        /// Address
        /// </summary>
        [DataMember(Name = "address")]
        public Address Address { get; set; }

        /// <summary>
        /// Credit card company
        /// </summary>
        [DataMember(Name = "clearingCompanyID")]
        public long? ClearingCompanyID { get; set; }

        /// <summary>
        /// Credit card company
        /// </summary>
        [DataMember(Name = "clearingCompany")]
        public string ClearingCompany { get; set; }
    }
}
