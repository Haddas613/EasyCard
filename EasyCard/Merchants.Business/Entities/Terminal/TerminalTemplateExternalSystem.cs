using Merchants.Shared.Enums;
using Newtonsoft.Json.Linq;
using Shared.Business.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Terminal
{
    public class TerminalTemplateExternalSystem
    {
        public long TerminalTemplateExternalSystemID { get; set; }

        public long ExternalSystemID { get; set; }

        public ExternalSystemTypeEnum Type { get; set; }

        public long TerminalTemplateID { get; set; }

        public TerminalTemplate TerminalTemplate { get; set; }

        public JObject Settings { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        public DateTime? Created { get; set; }
    }
}
