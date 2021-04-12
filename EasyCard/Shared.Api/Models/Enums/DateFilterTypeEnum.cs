using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared.Api.Models.Enums
{
    public enum DateFilterTypeEnum : short
    {
        [EnumMember(Value = "created")]
        Created = 0,

        [EnumMember(Value = "updated")]
        Updated = 1,
    }
}
