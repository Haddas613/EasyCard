using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Shared.Api.Models.Enums
{
    public enum QuickDateFilterTypeEnum : short
    {
        [EnumMember(Value = "last24Hours")]
        Last24Hours = 1,

        [EnumMember(Value = "lastWeek")]
        LastWeek = 2,

        [EnumMember(Value = "lastTwoWeeks")]
        LastTwoWeeks = 3,

        [EnumMember(Value = "lastMonth")]
        LastMonth = 4,

        [EnumMember(Value = "lastYear")]
        LastYear = 5
    }
}
