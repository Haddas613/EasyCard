using Merchants.Shared.Enums;
using Newtonsoft.Json.Linq;
using Shared.Business;
using Shared.Business.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Terminal
{
    public class TerminalExternalSystem : ITerminalEntity, IEntityBase<long>
    {
        // TODO: convert to guid
        public long TerminalExternalSystemID { get; set; }

        public long ExternalSystemID { get; set; }

        public ExternalSystemTypeEnum Type { get; set; }

        public Guid TerminalID { get; set; }

        public Terminal Terminal { get; set; }

        public JObject Settings { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        public DateTime? Created { get; set; }

        public long GetID()
        {
            return TerminalExternalSystemID;
        }
    }
}
