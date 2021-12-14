using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Integrations.EasyInvoice
{
    public class SetDocumentNumberRequest
    {
        [Required]
        public Guid TerminalID { get; set; }

        public int CurrentNum { get; set; }

        public string DocType { get; set; }
    }
}
