using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Invoicing
{
    public class CreateInvoicingConsumerRequest
    {
        public Guid ConsumerID { get; set; }

        public Guid TerminalID { get; set; }

        public string ConsumerName { get; set; }

        public string CellPhone { get; set; }

        public string Email { get; set; }

        public string NationalID { get; set; }
    }
}
