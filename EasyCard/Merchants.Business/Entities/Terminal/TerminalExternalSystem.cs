using Merchants.Business.Entities.Integration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Terminal
{
    public class TerminalExternalSystem
    {
        public long TerminalExternalSystemID { get; set; }

        public long ExternalSystemID { get; set; }

        public ExternalSystem ExternalSystem { get; set; }

        public long TerminalID { get; set; }

        public Terminal Terminal { get; set; }

        /// <summary>
        /// SHVA or other system terminal ID
        /// </summary>
        public string ExternalProcessorReference { get; set; }

        public JObject Settings { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        public DateTime? Created { get; set; }
    }
}
