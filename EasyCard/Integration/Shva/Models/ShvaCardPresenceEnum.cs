using System;
using System.Collections.Generic;
using System.Text;

namespace Shva.Models
{
    public enum ShvaCardPresenceEnum
    {
        /// <summary>
        /// Regular
        /// </summary>
        Magnetic = 00,

        /// <summary>
        /// Card not present
        /// </summary>
        TelephoneDdeal = 50,
        InternetDeal = 52
    }
}
