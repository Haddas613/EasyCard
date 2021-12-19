using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.User
{
    public class UserInfo
    {
        public Guid UserID { get; set; }

        public Guid? MerchantID { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public IEnumerable<string> Roles { get; set; }

        public IEnumerable<Guid?> Terminals { get; set; }
    }
}
