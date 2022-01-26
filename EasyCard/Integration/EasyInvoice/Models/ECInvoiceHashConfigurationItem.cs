using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Models
{
    public class ECInvoiceHashConfigurationItem
    {
        public string key { get; set; }
        public string debitAccount { get; set; }
        public string taxAccount { get; set; }
    }
}
