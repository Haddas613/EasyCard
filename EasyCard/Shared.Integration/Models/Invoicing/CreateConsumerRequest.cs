using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models.Invoicing
{
    public class CreateConsumerRequest
    {
        public object InvoiceingSettings { get; set; }

        public string ConsumerName { get; set; }

        public string CellPhone { get; set; }

        public string Email { get; set; }

        public string NationalID { get; set; }
    }
}
