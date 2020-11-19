using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Terminal
{
    public class TerminalInvoiceSettingsUpdate
    {
        [StringLength(250)]
        public string DefaultInvoiceSubject { get; set; }

        public InvoiceTypeEnum? DefaultInvoiceType { get; set; }
    }
}
