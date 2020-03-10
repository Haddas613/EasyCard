using System.Runtime.Serialization;

namespace ClearingHouse.Models
{
    /// <summary>
    /// KYC details approval statuses
    /// </summary>
    public enum KYCStatusEnum : int
    {
        /// <summary>
        /// Merchant created
        /// </summary>
        [EnumMember(Value = "initial")]
        Initial = 0,

        /// <summary>
        /// Some documents uploaded but some other required
        /// </summary>
        [EnumMember(Value = "inProgress")]
        InProgress = 1,

        /// <summary>
        /// All required provided documents approved
        /// </summary>
        [EnumMember(Value = "approved")]
        Approved = 2,

        /// <summary>
        /// KYC rejected
        /// </summary>
        [EnumMember(Value = "rejected")]
        Rejected = -1,
    }
}
