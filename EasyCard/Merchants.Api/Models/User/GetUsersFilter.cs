using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.User
{
    public class GetUsersFilter : FilterBase
    {
        public string Email { get; set; }
    }
}
