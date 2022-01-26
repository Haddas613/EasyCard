using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Ecwid.Models.Enums
{
    public enum EcwidPaymentStatusEnum
    {
        [EnumMember(Value = "AWAITING_PAYMENT")]
        AWAITING_PAYMENY = 0,

        [EnumMember(Value = "PAID")]
        PAID = 1,

        [EnumMember(Value = "CANCELLED")]
        CANCELLED = 2,

        [EnumMember(Value = "REFUNDED")]
        REFUNDED = 3,

        [EnumMember(Value = "INCOMPLETE")]
        INCOMPLETE = 4,
    }
}
