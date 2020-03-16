using System.Runtime.Serialization;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Create new Merchant request
    /// </summary>
    [DataContract]
    public partial class MerchantRequest
    {
        /// <summary>
        /// Payment Gateway registered ID in Clearing House system
        /// </summary>
        [DataMember(Name = "paymentGatewayID")]
        public int? PaymentGatewayID { get; set; }

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
        /// Address
        /// </summary>
        [DataMember(Name = "address")]
        public Address Address { get; set; }

        /// <summary>
        /// Credit card company
        /// </summary>
        [DataMember(Name = "clearingCompanyID")]
        public long? ClearingCompanyID { get; set; }
    }
}
