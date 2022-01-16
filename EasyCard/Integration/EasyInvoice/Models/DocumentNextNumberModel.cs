using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Models
{
    public class DocumentNextNumberModel
    {
        public string DocumentType { get; set; }

        public long NextDocumentNumber { get; set; }
    }
}
