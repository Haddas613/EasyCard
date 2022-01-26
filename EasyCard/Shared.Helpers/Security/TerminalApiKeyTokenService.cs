using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Options;

namespace Shared.Helpers.Security
{
    public class TerminalApiKeyTokenService : IWebApiClientTokenService
    {
        private readonly string privateKey;
        private readonly WebApiClient webApiClient;
        private readonly IdentityServerClientSettings configuration;

        private TokenResponse Token { get; set; }

        private DateTime ExpiryTime { get; set; }

        public TerminalApiKeyTokenService(string privateKey, WebApiClient webApiClient, IdentityServerClientSettings configuration)
        {
            this.privateKey = privateKey;
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

            var res = await webApiClient.PostRawFormRawResponse(configuration.Authority, "/connect/token",
                new Dictionary<string, string>
                {
                    { "client_id", "terminal" },
                    { "grant_type", "terminal_rest_api" },
                    { "authorizationKey", privateKey }
                });
            var tokenResponse = await TokenResponse.FromHttpResponseAsync<TokenResponse>(res);

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
