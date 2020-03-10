using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared.Helpers
{
    public enum CurrencyEnum
    {
        [EnumMember(Value = "ILS")]
        ILS = 0,

        [EnumMember(Value = "USD")]
        USD = 1,

        [EnumMember(Value = "EUR")]
        EUR = 2
    }
}
