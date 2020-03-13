using IdentityServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Data.Entities
{
    public class UserAudit
    {
        public long UserAuditID { get; set; }

        public string UserId { get; set; }

        public string Email { get; set; }

        public virtual ApplicationUser User { get; set; }

        public string OperationCode { get; set; }

        public string OperationDescription { get; set; }

        public string SourceIP { get; set; }

        public DateTime? OperationDate { get; set; }
    }
}
