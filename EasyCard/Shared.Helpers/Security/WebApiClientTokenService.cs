using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Options;

namespace Shared.Helpers.Security
{
    public class WebApiClientTokenService : IWebApiClientTokenService
    {
        private readonly IdentityServerClientSettings configuration;

        private readonly HttpClient httpClient;

        public virtual string TokenEndpoint { get; private set; }

        private TokenResponse Token { get; set; }

        private DateTime ExpiryTime { get; set; }

        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

        /// <summary>
        /// When set to true, only configuration string without /connect/token will be used as Address for get token request.
        /// </summary>
        private readonly bool customCredentialsTokenURi = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiClientTokenService"/> class.
        /// </summary>
        /// <param name="httpClient">http client</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="customCredentialsTokenURi">When set to true, only configuration string without /connect/token will be used as Address for get token request.</param>
        public WebApiClientTokenService(HttpClient httpClient, IOptions<IdentityServerClientSettings> configuration, bool customCredentialsTokenURi = false)
        {
            this.configuration = configuration.Value;
            this.httpClient = httpClient;
            this.customCredentialsTokenURi = customCredentialsTokenURi;
        }

        public virtual async Task<TokenResponse> GetToken(NameValueCollection headers = null)
        {
            //use token if it exists and is still fresh
            if (this.Token != null)
            {
                if (ExpiryTime >= DateTime.UtcNow)
                {
                    return Token;
                }
            }

            var request = new ClientCredentialsTokenRequest
            {
                Address = customCredentialsTokenURi ? configuration.Authority : $"{configuration.Authority}/connect/token",

                ClientId = configuration.ClientID,
                ClientSecret = configuration.ClientSecret,
                Scope = configuration.Scope,
            };

            if (headers != null)
            {
                foreach (var header in headers.AllKeys)
                {
                    request.Headers.Add(header, headers.GetValues(header).FirstOrDefault());
                }
            }

            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(request);

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
