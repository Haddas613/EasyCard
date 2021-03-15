using Merchants.Shared.Enums;
using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.User
{
    public class GetUsersFilter : FilterBase
    {
        public string Search { get; set; }

        public Guid? SearchGuid { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public UserStatusEnum? Status { get; set; }
    }
}
