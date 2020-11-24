using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Merchants.Shared.Models
{
    public class SystemInvoiceSettings
    {
        [StringLength(250)]
        public string DefaultInvoiceSubject { get; set; }

        public InvoiceTypeEnum? DefaultInvoiceType { get; set; }
    }
}
