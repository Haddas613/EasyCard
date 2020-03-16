using System.Runtime.Serialization;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Business Details
    /// </summary>
    [DataContract]
    public partial class BusinessDetails
    {
        /// <summary>
        /// Gets or Sets BusinessName
        /// </summary>
        [DataMember(Name = "businessName")]
        public string BusinessName { get; set; }

        /// <summary>
        /// Merchant&#39;s business Id
        /// </summary>
        [DataMember(Name = "businessId")]
        public string BusinessId { get; set; }

        /// <summary>
        /// Gets or Sets BusinessArea
        /// </summary>
        [DataMember(Name = "businessArea")]
        public string BusinessArea { get; set; }

        /// <summary>
        /// Bbusiness phone
        /// </summary>
        [DataMember(Name = "businessPhone")]
        public string BusinessPhone { get; set; }

        /// <summary>
        /// Corporate web site address
        /// </summary>
        [DataMember(Name = "webSite")]
        public string WebSite { get; set; }

        /// <summary>
        /// Corporate web site address
        /// </summary>
        [DataMember(Name = "businessType")]
        public string BusinessType { get; set; }
    }
}
