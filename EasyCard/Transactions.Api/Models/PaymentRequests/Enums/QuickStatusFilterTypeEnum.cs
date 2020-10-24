using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Transactions.Api.Models.PaymentRequests.Enums
{
    public enum QuickStatusFilterTypeEnum : short
    {
        [EnumMember(Value = "pending")]
        Pending = 1,

        [EnumMember(Value = "completed")]
        Completed = 2,

        [EnumMember(Value = "failed")]
        Failed = 3,

        [EnumMember(Value = "canceled")]
        Canceled = 4,

        [EnumMember(Value = "overdue")]
        Overdue = 5,
    }
}
