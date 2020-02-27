using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.User
{
    public class InviteUserRequest
    {
        public long MerchantID { get; set; }

        public string InviteMessage { get; set; }

        public string Email { get; set; }
    }
}
