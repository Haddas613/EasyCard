using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum DocumentOriginEnum : short
    {
        // TODO: do we need payment request here

        [EnumMember(Value = "UI")]
        UI = 0,

        [EnumMember(Value = "API")]
        API = 1,

        [EnumMember(Value = "checkout")]
        Checkout = 2,

        [EnumMember(Value = "billing")]
        Billing = 3,

        [EnumMember(Value = "device")]
        Device = 4,

        [EnumMember(Value = "paymentRequest")]
        PaymentRequest = 5
    }
}
