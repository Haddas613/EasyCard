using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum RepeatPeriodTypeEnum
    {
        [EnumMember(Value = "oneTime")]
        OneTime = 0,

        [EnumMember(Value = "montly")]
        Montly = 1,

        [EnumMember(Value = "biMontly")]
        BiMontly = 2,

        [EnumMember(Value = "quarter")]
        Quarter = 4,

        [EnumMember(Value = "year")]
        Year = 5,
    }
}
