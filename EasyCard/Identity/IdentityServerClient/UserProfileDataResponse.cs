using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServerClient
{
    public class UserProfileDataResponse
    {
        public string UserID { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public bool IsManager { get; set; }

        public IEnumerable<string> Roles { get; set; }

        public string Settings { get; set; }
    }
}
