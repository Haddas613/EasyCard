using System;
using System.Collections.Generic;
using System.Text;

namespace RapidOne.Models
{
    public class RapidInvoiceCreateDocumentResponse
    {
        public string error { get; set; }
        public DocumentItemModel[]  documents{get;set;}
    }
}
