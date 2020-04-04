using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public enum JDealTypeEnum : short
    {
        /// <summary>
        /// Check
        /// </summary>
        J2 = 1,

        /// <summary>
        /// Regular deal
        /// </summary>
        J4 = 0,

        /// <summary>
        /// Block card
        /// </summary>
        J5 = 2
    }
}
