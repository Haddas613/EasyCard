using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Transactions.Shared.Enums
{
    public enum PaymentTypeEnum : short
    {
        [EnumMember(Value = "card")]
        Card = 0,

        [EnumMember(Value = "check")]
        Check = 1,

        [EnumMember(Value = "cash")]
        Cash = 2,

        [EnumMember(Value = "bank")]
        Bank = 3,

        //TODO: uncomment when bitcoin is supported
        //[EnumMember(Value = "bitcoin")]
        //Bitcoin = 4
    }
}
