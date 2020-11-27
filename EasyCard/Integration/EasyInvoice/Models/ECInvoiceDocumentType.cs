using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Models
{
    public enum ECInvoiceDocumentType
    {
        INVOICE,
        INVOICE_WITH_PAYMENT_INFO,
        CREDIT_NOTE,
        PAYMENT_INFO,
        REFUND_INVOICE // TODO: check if it is supported by ECInvoice
    }
}
