using System;
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

        public WebApiClientTokenService(HttpClient httpClient, IOptions<IdentityServerClientSettings> configuration)
        {
            this.configuration = configuration.Value;
            this.httpClient = httpClient;
        }

        public virtual async Task<TokenResponse> GetToken()
        {
            if (string.IsNullOrWhiteSpace(this.TokenEndpoint))
            {
                await SemaphoreSlim.WaitAsync();
                {
                    if (string.IsNullOrWhiteSpace(this.TokenEndpoint))
                    {
                        try
                        {
                            var disco = await httpClient.GetDiscoveryDocumentAsync(configuration.Authority);
                            if (disco.IsError)
                            {
                                throw new ApplicationException($"Cannot resolve token endpoint: {disco.Error}");
                            }

                            this.TokenEndpoint = disco.TokenEndpoint;
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            SemaphoreSlim.Release();
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(this.TokenEndpoint))
            {
                throw new ApplicationException($"Cannot resolve token endpoint");
            }

            //use token if it exists and is still fresh
            if (this.Token != null)
            {
                if (ExpiryTime >= DateTime.UtcNow)
                {
                    return Token;
                }
            }

            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = $"{configuration.Authority}/connect/token",

                ClientId = configuration.ClientID,
                ClientSecret = configuration.ClientSecret,
                Scope = configuration.Scope
            });

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
