using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ClearingHouse.Models
{
    public enum TransactionTypeEnum : int
    {
        /// <summary>
        /// Regular transaction
        /// </summary>
        [EnumMember(Value = "regular")]
        Regular = 0,

        /// <summary>
        /// Installments payments
        /// </summary>
        [EnumMember(Value = "installments")]
        Installments = -1,

        /// <summary>
        /// Credit transaction
        /// </summary>
        [EnumMember(Value = "credit")]
        Credit = 2,

        /// <summary>
        /// Installments payments
        /// </summary>
        [EnumMember(Value = "refund")]
        Refund = -2,
    }
}
