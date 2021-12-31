using Bit.Configuration;
using IdentityModel.Client;
using Shared.Helpers;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace Bit.Services
{
    public class BitTokensService : IWebApiClientTokenService
    {
        private readonly WebApiClient webApiClient;
        private readonly BitGlobalSettings configuration;

        private TokenResponse Token { get; set; }

        private DateTime ExpiryTime { get; set; }

        public BitTokensService(WebApiClient webApiClient, BitGlobalSettings configuration)
        {
            this.webApiClient = webApiClient;
            this.configuration = configuration;
        }

        public async Task<TokenResponse> GetToken(NameValueCollection headers = null)
        {
            if (this.Token != null)
            {
                if (ExpiryTime >= DateTime.UtcNow)
                {
                    return Token;
                }
            }

            var res = await webApiClient.PostRawFormRawResponse(configuration.Authority, string.Empty,
                new Dictionary<string, string> {
                    { "client_id", configuration.ClientID },
                    { "client_secret", configuration.ClientSecret },
                    { "response_type", "token" },
                    { "scope", "bit_payment" },
                }, () => Task.FromResult(headers));

            var tokenResponse = await ProtocolResponse.FromHttpResponseAsync<TokenResponse>(res);

            if (tokenResponse.IsError)
            {
                throw new ApplicationException($"Could not retrieve token: {tokenResponse.Error} ({tokenResponse.ErrorType}). {tokenResponse.ErrorDescription}");
            }

            //set Token to the new token and set the expiry time to the new expiry time
            Token = tokenResponse;
            ExpiryTime = DateTime.UtcNow.AddSeconds(Token.ExpiresIn);

            //return fresh token
            return Token;
        }
    }
}
