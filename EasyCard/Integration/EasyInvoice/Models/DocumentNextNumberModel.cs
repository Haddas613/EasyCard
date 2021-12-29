using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Models
{

    public class DocumentNextNumberModel
    {
        public string documentType { get; set; }
        public Int64 nextDocumentNumber { get; set; }
    }
}
