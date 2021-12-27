using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace EasyInvoice.Models
{
   public  class ECInvoiceSetDocumentNumberRequest
    {
        public ECInvoiceDocumentType DocType { get; set; }
        public int CurrentNum { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        public EasyInvoiceTerminalSettings Terminal { get; set; }

    }
}
