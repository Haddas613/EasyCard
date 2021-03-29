using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServerClient
{
    public class CreateUserRequestModel
    {
        public string TerminalID { get; set; }

        public string MerchantID { get; set; }

        public string Email { get; set; }

        public string CellPhone { get; set; }

        public ICollection<string> Roles { get; set; }
    }
}
