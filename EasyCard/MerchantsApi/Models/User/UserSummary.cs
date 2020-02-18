using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantsApi.Models.User
{
    public class UserSummary
    {
        public string UserID { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public bool IsManager { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }
}
