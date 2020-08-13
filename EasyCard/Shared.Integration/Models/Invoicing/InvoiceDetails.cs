using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Integration.Models.Invoicing
{
    public class InvoiceDetails
    {
        public string InvoiceNumber { get; set; }

        public InvoiceTypeEnum InvoiceType { get; set; }

        public string InvoiceSubject { get; set; }

        public string[] SendCCTo { get; set; }

        public string DefaultInvoiceItem { get; set; }
    }
}
