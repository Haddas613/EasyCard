using IdentityServer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models
{
    public class TwoFactorAuthSettingsViewModel
    {

        public TwoFactorAuthTypeEnum TwoFactorAuthType { get; set; }

        public ApplicationUser UserInfo { get; set; }
    }
}
