using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.User
{
    public class UpdateUserRolesRequest
    {
        public Guid UserID { get; set; }

        public HashSet<string> Roles { get; set; }
    }
}
