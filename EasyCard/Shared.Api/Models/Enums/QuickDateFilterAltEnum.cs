using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Shared.Api.Models.Enums
{
    public enum QuickDateFilterAltEnum : short
    {
        [EnumMember(Value = "noComparison")]
        NoComparison = 0,

        [EnumMember(Value = "prevPeriod")]
        PrevPeriod = 1,

        [EnumMember(Value = "lastTwoWeeks")]
        LastTwoWeeks = 3,

        [EnumMember(Value = "lastMonth")]
        LastMonth = 4,

        [EnumMember(Value = "lastYear")]
        LastYear = 5
    }
}
