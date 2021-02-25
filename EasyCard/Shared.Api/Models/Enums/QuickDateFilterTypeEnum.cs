using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Shared.Api.Models.Enums
{
    public enum QuickDateFilterTypeEnum : short
    {
        [EnumMember(Value = "Last24Hours")]
        Last24Hours = 1,

        [EnumMember(Value = "LastWeek")]
        LastWeek = 2,

        [EnumMember(Value = "LastTwoWeeks")]
        LastTwoWeeks = 3,

        [EnumMember(Value = "LastMonth")]
        LastMonth = 4,

        [EnumMember(Value = "LastYear")]
        LastYear = 5
    }
}
