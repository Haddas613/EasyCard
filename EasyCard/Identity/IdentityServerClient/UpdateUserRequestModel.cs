using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServerClient
{
    public class UpdateUserRequestModel
    {
        public string UserID { get; set; }

        public ICollection<string> Roles { get; set; }
    }
}
