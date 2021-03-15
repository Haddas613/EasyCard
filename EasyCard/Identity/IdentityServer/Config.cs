using IdentityModel;
using IdentityServer4.Models;
using Shared.Api.Configuration;
using Shared.Helpers.Security;
using System.Collections.Generic;

namespace IdentityServer
{
    public class Config
    {
        private readonly ApiSettings apiSettings;
        private readonly IdentityServerClientSettings identitySettings;

        public Config(ApiSettings apiSettings, IdentityServerClientSettings identitySettings)
        {
            this.apiSettings = apiSettings;
            this.identitySettings = identitySettings;
        }

        public IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource("roles", "Roles", new[] { JwtClaimTypes.Role,  Shared.Helpers.Security.Claims.FirstNameClaim, Shared.Helpers.Security.Claims.LastNameClaim }),
            };

        public IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("merchants_api", "Merchants Api", new[] { JwtClaimTypes.Subject, JwtClaimTypes.Name, JwtClaimTypes.Role, JwtClaimTypes.Audience, Shared.Helpers.Security.Claims.MerchantIDClaim }),
                new ApiScope("transactions_api", "Transactions Api", new[] { JwtClaimTypes.Subject, JwtClaimTypes.Name, JwtClaimTypes.Role, Shared.Helpers.Security.Claims.TerminalIDClaim, Shared.Helpers.Security.Claims.MerchantIDClaim, Shared.Helpers.Security.Claims.FirstNameClaim, Shared.Helpers.Security.Claims.LastNameClaim }),
                new ApiScope("management_api", "User Management")
            };

        public IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource("merchants_api", "Merchants Api", new[] { JwtClaimTypes.Subject, JwtClaimTypes.Name, JwtClaimTypes.Role, JwtClaimTypes.Audience, Shared.Helpers.Security.Claims.MerchantIDClaim }),
                new ApiResource("transactions_api", "Transactions Api", new[] { JwtClaimTypes.Subject, JwtClaimTypes.Name, JwtClaimTypes.Role, Shared.Helpers.Security.Claims.TerminalIDClaim, Shared.Helpers.Security.Claims.MerchantIDClaim }),
                new ApiResource("management_api", "User Management")
                {
                    ApiSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                },
            };

        public IEnumerable<Client> Clients =>
            new Client[]
            {
                // SPA client using code flow + pkce
                new Client
                {
                    ClientId = "merchant_frontend",
                    ClientName = "Merchant's Frontend",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RequireConsent = false,
                    RedirectUris =
                    {
                        "http://localhost:8080/",
                        "http://localhost:8080/callback.html",
                        "http://localhost:8080/silent-renew.html",

                        $"{apiSettings.MerchantProfileURL}",
                        $"{apiSettings.MerchantProfileURL}/callback.html",
                        $"{apiSettings.MerchantProfileURL}/silent-renew.html",
                    },

                    PostLogoutRedirectUris =
                    {
                        $"{identitySettings.Authority}",
                        $"{apiSettings.MerchantProfileURL}/",
                        "http://localhost:8080/",
                    },
                    AllowedCorsOrigins = { "http://localhost:8080", "https://localhost:44339", $"{apiSettings.MerchantProfileURL}", },
                    AllowAccessTokensViaBrowser = true,
                    AllowedScopes = { "openid", "profile", "transactions_api", "roles" },
                    AlwaysIncludeUserClaimsInIdToken = true,
                },

                // SPA client using code flow + pkce
                new Client
                {
                    ClientId = "admin_frontend",
                    ClientName = "Admin's Frontend",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RequireConsent = false,
                    RedirectUris =
                    {
                        "http://localhost:8081/",

                        "http://localhost:8081/callback.html",
                        "http://localhost:8081/silent-renew.html",

                        $"{apiSettings.MerchantsManagementApiAddress}",
                        $"{apiSettings.MerchantsManagementApiAddress}/callback.html",
                        $"{apiSettings.MerchantsManagementApiAddress}/silent-renew.html",
                    },

                    PostLogoutRedirectUris =
                    {
                        $"{identitySettings.Authority}",
                        $"{apiSettings.MerchantsManagementApiAddress}/",
                        "http://localhost:8081/",
                    },
                    AllowedCorsOrigins = { $"{apiSettings.MerchantsManagementApiAddress}", "http://localhost:8081", "https://localhost:44390" },
                    AllowAccessTokensViaBrowser = true,
                    AllowedScopes = { "openid", "profile", "transactions_api", "merchants_api", "roles" },
                    AlwaysIncludeUserClaimsInIdToken = true,
                },
                new Client
                {
                    ClientId = "management_api",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    ClientSecrets =
                    {
                        new Secret(identitySettings.ClientSecret.Sha256()), new Secret(identitySettings.ClientSecretAlt.Sha256())
                    },
                    AllowedScopes = { "management_api", "merchants_api" },
                    AccessTokenType = AccessTokenType.Jwt,
                    AccessTokenLifetime = 3600 * 24 // TODO: config
                },
                new Client
                {
                    ClientId = "checkout_portal",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    ClientSecrets =
                    {
                        new Secret("yuwsCT8cbJVgw2W6".Sha256()) // TODO
                    },
                    AllowedScopes = { "transactions_api" },
                    AccessTokenType = AccessTokenType.Jwt,
                    AccessTokenLifetime = 3600 * 24 // TODO: config
                },
                new Client
                {
                    ClientId = "terminal",

                    RequireClientSecret = false,

                    //ClientSecrets = new List<Secret>
                    //{
                    //    new Secret("secret".Sha256())
                    //},

                    AllowedGrantTypes = { "terminal_rest_api" },

                    AllowedScopes = new List<string>
                    {
                        "transactions_api"
                    },

                    AccessTokenLifetime = 3600 * 24 // TODO: config
                }
            };
    }
}