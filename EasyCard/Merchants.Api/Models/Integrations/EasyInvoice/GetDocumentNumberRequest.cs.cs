using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Integrations.EasyInvoice
{
    public class GetDocumentNumberRequest
    {
        [Required]
        public Guid TerminalID { get; set; }


        public string DocType { get; set; }
    }
}
