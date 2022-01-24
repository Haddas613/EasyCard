using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.Security
{
    public class TerminalApiKeyTokenServiceFactory : ITerminalApiKeyTokenServiceFactory
    {
        private readonly IdentityServerClientSettings apiSettings;

        private readonly WebApiClient webApiClient = new WebApiClient();

        public TerminalApiKeyTokenServiceFactory(IOptions<IdentityServerClientSettings> apiSettings)
        {
            this.apiSettings = apiSettings.Value;
        }

        public IWebApiClientTokenService CreateTokenService(string privateKey)
        {
            return new TerminalApiKeyTokenService(privateKey, webApiClient, apiSettings);
        }
    }
}
