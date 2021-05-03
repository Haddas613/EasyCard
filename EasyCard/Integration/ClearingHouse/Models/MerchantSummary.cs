using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Merchant list view
    /// </summary>
    [DataContract]
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public partial class MerchantSummary
    {
        /// <summary>
        /// Gets or Sets MerchantID
        /// </summary>
        [DataMember(Name = "merchantID")]
        public long? MerchantID { get; set; }

        /// <summary>
        /// Initial record
        /// </summary>
        [DataMember(Name = "parentMerchantID")]
        public long? ParentMerchantID { get; set; }

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
        /// Gets or Sets RiskRate
        /// </summary>
        [DataMember(Name = "riskRate")]
        public int? RiskRate { get; set; }

        /// <summary>
        /// Gets or Sets KycApprovalStatus
        /// </summary>
        [DataMember(Name = "kycApprovalStatus")]
        [JsonConverter(typeof(StringEnumConverter))]
        public KYCStatusEnum? KycApprovalStatus { get; set; }

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
        /// Gets or Sets email
        /// </summary>
        [DataMember(Name = "email")]
        public string Email { get; set; }

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
        /// Credit card company
        /// </summary>
        [DataMember(Name = "clearingCompany")]
        public string ClearingCompany { get; set; }

        /// <summary>
        /// Visa Id
        /// </summary>
        [DataMember(Name = "clearingCompanyReference")]
        public string ClearingCompanyReference { get; set; }

        /// <summary>
        /// Discount of installments
        /// </summary>
        [DataMember(Name = "nikionEnabled")]
        public bool NikionEnabled { get; set; }

        /// <summary>
        /// Block any transaction
        /// </summary>
        [DataMember(Name = "blockTransactions")]
        public bool BlockTransactions { get; set; }

        [DataMember(Name = "ravMotav")]
        public bool RavMotav { get; set; }

        [DataMember(Name = "isDisable")]
        public bool IsDisable { get; set; }
    }
}
