using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared.Integration.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SolekEnum : short
    {
        [EnumMember(Value = "UNKNOWN")]
        UNKNOWN = 0,

        [EnumMember(Value = "VISA")]
        VISA = 2,

        [EnumMember(Value = "AMEX")]
        AMEX = 4,

        //[EnumMember(Value = "DISCOVER")]
        //DISCOVER,

        [EnumMember(Value = "JCB")]
        JCB = 5,

        [EnumMember(Value = "DINERS_CLUB")]
        DINERS_CLUB = 3,

        [EnumMember(Value = "ISRACARD")]
        ISRACARD = 1,

        [EnumMember(Value = "LEUMI_CARD")]
        LEUMI_CARD = 6,

        [EnumMember(Value = "OTHER")]
        OTHER = 7,

        [EnumMember(Value = "MASTERCARD")]
        MASTERCARD = 8
    }
}
