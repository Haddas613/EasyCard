using System.Runtime.Serialization;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Payment gateway details
    /// </summary>
    [DataContract]
    public partial class PaymentGatewayMerchantDetails
    {
        /// <summary>
        /// credit card company who will process the actual transactions (Isracard or Visa CAL)
        /// </summary>
        [DataMember(Name = "creditCardVendor")]
        public string CreditCardVendor { get; set; }

        /// <summary>
        /// Gets or Sets TerminalReference
        /// </summary>
        [DataMember(Name = "terminalReference")]
        public string TerminalReference { get; set; }

        /// <summary>
        /// Gets or Sets AdditionalClearingHouse
        /// </summary>
        [DataMember(Name = "additionalClearingHouse")]
        public string AdditionalClearingHouse { get; set; }

        /// <summary>
        /// Gets or Sets AdditionalClearingHouseApprovedByCreditCompany
        /// </summary>
        [DataMember(Name = "additionalClearingHouseApprovedByCreditCompany")]
        public bool? AdditionalClearingHouseApprovedByCreditCompany { get; set; }
    }
}
