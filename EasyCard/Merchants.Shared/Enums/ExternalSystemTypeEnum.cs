using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Merchants.Shared.Enums
{
    public enum ExternalSystemTypeEnum
    {
        [EnumMember(Value = "processor")]
        Processor,

        [EnumMember(Value = "aggregator")]
        Aggregator,

        [EnumMember(Value = "invoicing")]
        Invoicing,

        [EnumMember(Value = "marketer")]
        Marketer,
    }
}
