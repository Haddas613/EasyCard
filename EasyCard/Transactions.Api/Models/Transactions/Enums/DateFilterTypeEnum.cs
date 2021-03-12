using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions.Enums
{
    public enum DateFilterTypeEnum : short
    {
        [EnumMember(Value = "created")]
        Created = 0,

        [EnumMember(Value = "updated")]
        Updated = 1,
    }
}
