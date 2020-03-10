using System;
using System.Runtime.Serialization;
using System.Text;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Credit Card Account
    /// </summary>
    [DataContract]
    public partial class CreditCardAccount
    {
        /// <summary>
        /// Gets or Sets CardNumber
        /// </summary>
        [DataMember(Name = "cardNumber")]
        public string CardNumber { get; set; }

        /// <summary>
        /// Gets or Sets CardExpiration
        /// </summary>
        [DataMember(Name = "cardExpiration")]
        public string CardExpiration { get; set; }

        /// <summary>
        /// Gets or Sets CardOwnerName
        /// </summary>
        [DataMember(Name = "cardOwnerName")]
        public string CardOwnerName { get; set; }

        /// <summary>
        /// Gets or Sets CardOwnerNationalId
        /// </summary>
        [DataMember(Name = "cardOwnerNationalId")]
        public string CardOwnerNationalId { get; set; }
    }
}
