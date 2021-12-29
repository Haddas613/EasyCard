using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Helpers;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace DesktopEasyCardConvertorECNG
{
    public class TokensService : IWebApiClientTokenService
    {
        private readonly string privateKey;
        private readonly WebApiClient webApiClient;
        private readonly Configuration configuration;

        internal TokensService(string privateKey, WebApiClient webApiClient, Configuration configuration)
        {
            this.privateKey = privateKey;
            this.webApiClient = webApiClient;
            this.configuration = configuration;
        }

        public async Task<TokenResponse> GetToken(NameValueCollection headers = null)
        {
            var tokenResponse = await webApiClient.PostRawFormRawResponse(configuration.IdentityApiAddress, "/connect/token",
                new Dictionary<string, string> { { "client_id", "terminal" }, { "grant_type", "terminal_rest_api" }, { "authorizationKey", privateKey } });
            var res = await TokenResponse.FromHttpResponseAsync<TokenResponse>(tokenResponse);
            
            return res;
        }
    }
}
