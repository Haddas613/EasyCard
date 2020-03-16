using System.Runtime.Serialization;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Operation Status
    /// </summary>
    /// <value>Operation Status</value>
    public enum StatusEnum
    {
        /// <summary>
        /// Enum SuccessEnum for "success"
        /// </summary>
        [EnumMember(Value = "success")]
        Success = 0,

        /// <summary>
        /// Enum WarningEnum for "warning"
        /// </summary>
        [EnumMember(Value = "warning")]
        Warning = 2,

        /// <summary>
        /// Enum ErrorEnum for "error"
        /// </summary>
        [EnumMember(Value = "error")]
        Error = 1
    }
}
