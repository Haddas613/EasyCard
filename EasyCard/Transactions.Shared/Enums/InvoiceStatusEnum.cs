using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum InvoiceStatusEnum : short
    {
        [EnumMember(Value = "initial")]
        Initial = 0,

        [EnumMember(Value = "sending")]
        Sending = 1,

        [EnumMember(Value = "sent")]
        Sent = 2,

        [EnumMember(Value = "sendingFailed")]
        SendingFailed = -1
    }
}
