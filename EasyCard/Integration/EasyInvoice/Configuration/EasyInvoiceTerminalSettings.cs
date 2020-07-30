using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice
{
    public class EasyInvoiceTerminalSettings
    {
        public string EasyInvoiceRequestsLogStorageTable { get; set; } = "easyinvoice";

        //public string EasyInvoiceBaseUrl { get; set; }

        public string KeyStorePassword { get; set; }

        public string AuthToken { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string InvoiceDescription { get; set; }

        public string ResendInvoiceSubject { get; set; }

        public string ResendInvoiceBody { get; set; }
    }
}
