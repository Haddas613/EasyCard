using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared.Integration.Models
{
    /// <summary>
    /// Type of Deal. optional values are; J2 for Check,J4 for Charge, J5 for Block card
    /// </summary>
    public enum JDealTypeEnum : short
    {
        /// <summary>
        /// Check
        /// </summary>
        [EnumMember(Value = "J2")]
        J2 = 1,

        /// <summary>
        /// Regular deal
        /// </summary>
        [EnumMember(Value = "J4")]
        J4 = 0,

        /// <summary>
        /// Block card
        /// </summary>
        [EnumMember(Value = "J5")]
        J5 = 2
    }
}
