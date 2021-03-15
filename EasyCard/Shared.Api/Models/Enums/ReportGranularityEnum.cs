using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared.Api.Models.Enums
{
    public enum ReportGranularityEnum : short
    {
        [EnumMember(Value = "date")]
        Date = 0,

        [EnumMember(Value = "week")]
        Week = 1,

        [EnumMember(Value = "month")]
        Month = 2,
    }
}
