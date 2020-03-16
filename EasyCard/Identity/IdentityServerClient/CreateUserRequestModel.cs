using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServerClient
{
    public class CreateUserRequestModel
    {
        public long TerminalID { get; set; }

        public long MerchantID { get; set; }

        public string Email { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }
}
