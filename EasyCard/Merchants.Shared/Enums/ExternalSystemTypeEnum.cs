using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Merchants.Shared.Enums
{
    public enum ExternalSystemTypeEnum
    {
        [EnumMember(Value = "processor")]
        Processor = 1,

        [EnumMember(Value = "aggregator")]
        Aggregator = 2,

        [EnumMember(Value = "invoicing")]
        Invoicing = 3,

        [EnumMember(Value = "marketer")]
        Marketer = 4,
    }
}
