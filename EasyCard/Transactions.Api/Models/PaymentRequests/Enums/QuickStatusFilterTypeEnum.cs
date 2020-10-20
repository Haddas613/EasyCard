using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Transactions.Api.Models.PaymentRequests.Enums
{
    public enum QuickStatusFilterTypeEnum : short
    {
        [EnumMember(Value = "Pending")]
        Pending = 1,

        [EnumMember(Value = "Completed")]
        Completed = 2,

        [EnumMember(Value = "Failed")]
        Failed = 3,

        [EnumMember(Value = "Canceled")]
        Canceled = 4,
    }
}
