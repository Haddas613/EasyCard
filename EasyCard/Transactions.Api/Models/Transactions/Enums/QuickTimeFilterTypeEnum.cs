using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions.Enums
{
    public enum QuickTimeFilterTypeEnum : short
    {
        [EnumMember(Value = "None")]
        None = 0,

        [EnumMember(Value = "Last5Minutes")]
        Last5Minutes = 1,

        [EnumMember(Value = "Last15Minutes")]
        Last15Minutes = 2,

        [EnumMember(Value = "Last30Minutes")]
        Last30Minutes = 3,

        [EnumMember(Value = "LastHour")]
        LastHour = 4,

        [EnumMember(Value = "Last24Hours")]
        Last24Hours = 5
    }
}
