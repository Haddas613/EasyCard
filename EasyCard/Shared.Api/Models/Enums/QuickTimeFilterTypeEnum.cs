using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Shared.Api.Models.Enums
{
    public enum QuickTimeFilterTypeEnum : short
    {
        [EnumMember(Value = "last5Minutes")]
        Last5Minutes = 1,

        [EnumMember(Value = "last15Minutes")]
        Last15Minutes = 2,

        [EnumMember(Value = "last30Minutes")]
        Last30Minutes = 3,

        [EnumMember(Value = "lastHour")]
        LastHour = 4,

        [EnumMember(Value = "last24Hours")]
        Last24Hours = 5
    }
}
