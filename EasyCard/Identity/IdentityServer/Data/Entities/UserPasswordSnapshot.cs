using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Data.Entities
{
    public class UserPasswordSnapshot
    {
        public long UserPasswordSnapshotID { get; set; }

        public string UserId { get; set; }

        public string HashedPassword { get; set; }

        public string SecurityStamp { get; set; }

        public DateTime Created { get; set; }
    }
}
