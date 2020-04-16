using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions.Enums
{
    public enum DateFilterTypeEnum : short
    {
        [EnumMember(Value = "Created")]
        Created = 0,

        [EnumMember(Value = "Updated")]
        Updated = 1,
    }
}
