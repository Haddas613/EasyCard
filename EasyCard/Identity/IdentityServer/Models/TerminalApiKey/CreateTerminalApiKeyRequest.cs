using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models.TerminalApiKey
{
    public class CreateTerminalApiKeyRequest
    {
        public Guid TerminalID { get; set; }
    }
}
