using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Terminal
{
    public class ExternalSystemRequest
    {
        public Guid TerminalID { get; set; }

        public long ExternalSystemID { get; set; }

        /// <summary>
        /// SHVA or other system terminal ID
        /// </summary>
        public string ExternalProcessorReference { get; set; }

        public JObject Settings { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        public DateTime? Created { get; set; }

        public NayaxTerminalInfo DeviceTerminal { get; set; }
    }

    public class NayaxTerminalInfo
    {
         public string PosName { get; set; }

        public long? TerminalID { get; set; }
    }
}
