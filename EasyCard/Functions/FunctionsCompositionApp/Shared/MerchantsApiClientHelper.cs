using Merchants.Api.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api.Configuration;
using Shared.Helpers;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace FunctionsCompositionApp.Shared
{
    public static class MerchantsApiClientHelper
    {
        public static IMerchantsApiClient GetMerchantsApiClient(ILogger logger, IConfigurationRoot config)
        {
            var section = config.GetSection("IdentityServerClient");
            logger.LogInformation($"Section {section?.Value}");

            var identitySection = config.GetSection("IdentityServerClient")?.Get<IdentityServerClientSettings>();
            var apiCfgsection = config.GetSection("API")?.Get<ApiSettings>();

            logger.LogInformation($"Authority {identitySection?.Authority}");
            logger.LogInformation($"ClientID {identitySection?.ClientID}");

            var cfg = Options.Create(identitySection);
            var apiCfg = Options.Create(apiCfgsection);
            var webApiClient = new WebApiClient();
            var tokenService = new WebApiClientTokenService(webApiClient.HttpClient, cfg);

            return new MerchantsApiClient(webApiClient, tokenService, apiCfg);
        }
    }
}
