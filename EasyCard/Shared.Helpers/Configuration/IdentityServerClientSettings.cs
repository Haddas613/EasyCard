using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.Security
{
    public class IdentityServerClientSettings
    {
        /// <summary>
        /// Identity Server Address
        /// </summary>
        public string Authority { get; set; }

        public string ClientID { get; set; }

        public string ClientSecret { get; set; }

        public string ClientSecretAlt { get; set; }

        public string Scope { get; set; }
    }
}
