using IdentityServer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models
{
    public class LoginWith2faTypeModel
    {
        public TwoFactorAuthTypeEnum LoginType { get; set; }
    }
}
