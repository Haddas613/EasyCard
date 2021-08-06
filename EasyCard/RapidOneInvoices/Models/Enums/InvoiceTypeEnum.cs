using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RapidOneInvoices.Models.Enums
{
    /// <summary>
    /// CreditCustomer refund for receipt
    /// 
    /// </summary>
    public enum InvoiceTypeEnum : short
    {
        [EnumMember(Value = "Invoice")]
        Invoice = 1,

        [EnumMember(Value = "InvoiceReceipt")]
        InvoiceReceipt = 2,

        [EnumMember(Value = "InvoiceRefund")]
        InvoiceRefund = 3,

        [EnumMember(Value = "Receipt")]
        Receipt = 4,

        [EnumMember(Value = "CreditCustomer")]
        CreditCustomer = 5
    }
}
