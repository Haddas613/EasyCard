using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum RepeatPeriodTypeEnum
    {
        [EnumMember(Value = "day")]
        Day = 0,

        [EnumMember(Value = "week")]
        Week = 1,

        [EnumMember(Value = "month")]
        Month = 2,

        [EnumMember(Value = "year")]
        Year = 3
    }
}
