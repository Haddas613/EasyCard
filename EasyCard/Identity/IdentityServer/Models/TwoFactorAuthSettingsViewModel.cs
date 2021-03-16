using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models
{
    public class TwoFactorAuthSettingsViewModel
    {

        public string Type { get; set; }

        public ApplicationUser UserInfo { get; set; }
    }
}
