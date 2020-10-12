using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared.Integration.Models.Invoicing
{
    public enum InvoiceTypeEnum : short
    {
        [EnumMember(Value = "invoice")]
        Invoice,

        [EnumMember(Value = "invoiceWithPaymentInfo")]
        InvoiceWithPaymentInfo,

        [EnumMember(Value = "creditNote")]
        CreditNote,

        [EnumMember(Value = "paymentInfo")]
        PaymentInfo
    }
}
