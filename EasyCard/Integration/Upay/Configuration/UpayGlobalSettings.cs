using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace Upay
{
    public class UpayGlobalSettings : IdentityServerClientSettings
    {
        public string ApiBaseAddress { get; set; }
        public string ApiCommitAddress { get; set; }
        public string ApiKey { get; set; }
    }
}
