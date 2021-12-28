using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Models
{
    public class DocumentTypeModel
    {
        public string INVOICE { get; set; }
        public string INVOICE_WITH_PAYMENT_INFO { get; set; }
        public string CREDIT_NOTE { get; set; }
        public string PAYMENT_INFO { get; set; }
        public string REFUND_INVOICE_WITH_PAYMENT_INFO { get; set; }
    }
}
