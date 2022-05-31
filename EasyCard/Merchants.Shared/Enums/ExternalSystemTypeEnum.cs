using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Merchants.Shared.Enums
{
    /// <summary>
    /// External System options are: Processor, Aggregator, Invoicing, Marketer, PinpadProcessor, VirtualWalletProcessor
    /// </summary>
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

        [EnumMember(Value = "pinpadProcessor")]
        PinpadProcessor = 5,

        [EnumMember(Value = "virtualWalletProcessor")]
        VirtualWalletProcessor = 6
    }
}
