using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shared.Helpers.Models;
using Shared.Api.Models.Binding;

namespace Transactions.Api.Models.Invoicing
{
    public class CreateInvoicingConsumerRequest
    {
        public Guid ConsumerID { get; set; }

        public Guid TerminalID { get; set; }

        public string ConsumerName { get; set; }

        public string CellPhone { get; set; }

        public string Email { get; set; }

        [JsonConverter(typeof(TrimmingJsonConverter))]
        [IsraelNationalIDValidator]
        public string NationalID { get; set; }
    }
}
