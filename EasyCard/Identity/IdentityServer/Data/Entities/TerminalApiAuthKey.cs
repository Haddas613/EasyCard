using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Data.Entities
{
    public class TerminalApiAuthKey : IEntityBase
    {
        public long TerminalApiAuthKeyID { get; set; }

        public long TerminalID { get; set; }

        public string AuthKey { get; set; }

        public DateTime Created { get; set; }

        public long GetID() => TerminalApiAuthKeyID;
    }
}
