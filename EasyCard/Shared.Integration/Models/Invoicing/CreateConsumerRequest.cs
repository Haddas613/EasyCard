using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Newtonsoft.Json;
using Shared.Api.Models.Binding;
using Shared.Helpers.Models;

namespace Shared.Integration.Models.Invoicing
{
    public class CreateConsumerRequest
    {
        public object InvoiceingSettings { get; set; }

        public string ConsumerName { get; set; }

        public string CellPhone { get; set; }

        public string Email { get; set; }

        [JsonConverter(typeof(TrimmingJsonConverter))]
        [Required(AllowEmptyStrings = false)]
        [IsraelNationalIDValidator]
        public string NationalID { get; set; }
    }
}
