using System.Runtime.Serialization;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Transaction statuses
    /// </summary>
    public enum TransactionStatusEnum : int
    {
        /// <summary>
        /// Transaction created
        /// </summary>
        [EnumMember(Value = "initial")]
        Initial = 0,

        /// <summary>
        /// Transaction approved by Shva but expected Visa approval
        /// </summary>
        [EnumMember(Value = "expectedDeal")]
        ExpectedDeal = 1,

        /// <summary>
        /// Completed approval
        /// </summary>
        [EnumMember(Value = "committed")]
        Committed = 2,

        /// <summary>
        /// Transaction rejected (by payment gateway, Shva or Visa)
        /// </summary>
        [EnumMember(Value = "rejected")]
        Rejected = -1,

        /// <summary>
        /// Chargeback process initiated
        /// </summary>
        [EnumMember(Value = "chargebackInProgres")]
        ChargebackInProgres = -2,

        /// <summary>
        /// Chargeback process completed
        /// </summary>
        [EnumMember(Value = "chargebackDone")]
        ChargebackDone = -3
    }
}