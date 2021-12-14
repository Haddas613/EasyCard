using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared.Integration.Models
{
    public enum PaymentTypeEnum : short
    {
        [EnumMember(Value = "card")]
        Card = 0,

        [EnumMember(Value = "cheque")]
        Cheque = 1,

        [EnumMember(Value = "cash")]
        Cash = 2,

        [EnumMember(Value = "bank")]
        Bank = 3,

        [EnumMember(Value = "invoice-only")]
        InvoiceOnly = 5,

        //TODO: uncomment when bitcoin is supported
        //[EnumMember(Value = "bitcoin")]
        //Bitcoin = 4

    }
}
