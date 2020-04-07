using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Transactions.Shared.Enums
{
    /// <summary>
    /// Is the card physically scanned
    /// </summary>
    public enum CardPresenceEnum : short
    {
        /// <summary>
        /// Magnetic
        /// </summary>
        [EnumMember(Value = "regular")]
        Regular = 0,

        /// <summary>
        /// Telephone deal
        /// </summary>
        [EnumMember(Value= "cardNotPresent")]
        CardNotPresent = 1
    }
}
