using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Ecwid.Api.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EcwidOrderStatusEnum
    {
        [EnumMember(Value = "PAID")]
        PAID = 0,

        [EnumMember(Value = "AWAITING_PAYMENT")]
        AWAITING_PAYMENT = 1,

        [EnumMember(Value = "INCOMPLETE")]
        INCOMPLETE = 2,

        [EnumMember(Value = "CANCELLED")]
        CANCELLED = 3,
    }
}
