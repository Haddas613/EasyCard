using System;
using System.Collections.Generic;
using System.Text;

namespace RapidOneInvoices.Models
{
    public class RapidInvoiceCreateDocumentResponse
    {
        public string error { get; set; }
        public DocumentItemModel[]  documents{get;set;}
    }
}
