using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Models
{
   public  class ECInvoiceSetDocumentNumberRequest
    {
        public ECInvoiceDocumentType DocType { get; set; }
        public int CurrentNum { get; set; }
    }
}
