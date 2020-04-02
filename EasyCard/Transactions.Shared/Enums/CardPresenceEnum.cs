using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Enums
{
    /// <summary>
    /// Is the card physically scanned
    /// </summary>
    public enum CardPresenceEnum
    {
        /// <summary>
        /// Magnetic
        /// </summary>
        Regular = 0,

        /// <summary>
        /// Telephone deal
        /// </summary>
        CardNotPresent = 1
    }
}
