using Merchants.Shared.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Terminal
{
    public class TerminalExternalSystemDetails
    {
        public long ExternalSystemID { get; set; }

        public ExternalSystemSummary ExternalSystem { get; set; }

        public JObject Settings { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        public DateTime? Created { get; set; }
    }
}
