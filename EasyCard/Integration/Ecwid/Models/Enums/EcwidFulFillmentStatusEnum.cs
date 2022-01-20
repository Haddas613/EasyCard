using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Ecwid.Models.Enums
{
    public enum EcwidFulfillmentStatusEnum
    {
        [EnumMember(Value = "AWAITING_PROCESSING")]
        AWAITING_PROCESSING = 0,

        [EnumMember(Value = "PROCESSING")]
        PROCESSING = 1,

        [EnumMember(Value = "SHIPPED")]
        SHIPPED = 2,

        [EnumMember(Value = "DELIVERED")]
        DELIVERED = 3,

        [EnumMember(Value = "WILL_NOT_DELIVER")]
        WILL_NOT_DELIVER = 4,

        [EnumMember(Value = "RETURNED")]
        RETURNED = 5,
    }
}
