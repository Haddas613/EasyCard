using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum InvoiceStatusEnum : short
    {
        Initial = 0,
        Sending = 1,
        Sent = 2,
        SendingFailed = -1
    }
}
