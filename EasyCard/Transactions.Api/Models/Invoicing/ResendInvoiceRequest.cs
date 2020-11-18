using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Invoicing
{
    public class ResendInvoiceRequest
    {
        /// <summary>
        /// EasyCard terminal reference
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public Guid TerminalID { get; set; }

        /// <summary>
        /// IDs of invoices which need to be resend
        /// </summary>
        [Required]
        public IEnumerable<Guid> InvoicesID { get; set; }
    }
}
