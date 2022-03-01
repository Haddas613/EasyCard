using Shared.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Invoicing
{
    public class CreateInvoicingConsumerResponse
    {
        public string ConsumerReference { get; set; }

        public string Origin { get; set; }

        public StatusEnum Status { get; set; }

        public string Message { get; set; }
    }
}
