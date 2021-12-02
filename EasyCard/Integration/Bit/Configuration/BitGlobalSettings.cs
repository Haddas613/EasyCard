using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.Configuration
{
    public class BitGlobalSettings : IdentityServerClientSettings
    {
        public string BaseUrl { get; set; }

        public string OcpApimSubscriptionKey { get; set; }
    }
}
