using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Models
{
    public class ECInvoiceGetDocumentNumberRequest
    {
        public ECInvoiceDocumentType DocType { get; set; }

        public EasyInvoiceTerminalSettings Terminal { get; set; }
    }
}
