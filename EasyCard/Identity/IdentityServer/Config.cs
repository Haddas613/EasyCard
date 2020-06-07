using IdentityModel;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource("merchants_api", "Merchants Api", new[] { JwtClaimTypes.Subject, JwtClaimTypes.Name, JwtClaimTypes.Role, Shared.Helpers.Security.Claims.MerchantIDClaim }),
                new ApiResource("transactions_api", "Transactions Api", new[] { JwtClaimTypes.Subject, JwtClaimTypes.Name, JwtClaimTypes.Role, Shared.Helpers.Security.Claims.TerminalIDClaim, Shared.Helpers.Security.Claims.MerchantIDClaim }),
                new ApiResource("management_api", "User Management")
                {
                    ApiSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                },
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // SPA client using code flow + pkce
                new Client
                {
                    ClientId = "merchant_frontend",
                    ClientName = "Merchant's Frontend",

                    //ClientUri = "http://identityserver.io",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RequireConsent = false,
                    RedirectUris =
                    {
                        "http://localhost:8080/",
                        "http://localhost:8080/auth/signinpop/main",
                        "http://localhost:8080/auth/signinwin/main",
                        "http://localhost:8080/auth/signinsilent/main",

                        "https://localhost:44339/",
                        "https://localhost:44339/auth/signinpop/main",
                        "https://localhost:44339/auth/signinwin/main",
                        "https://localhost:44339/auth/signinsilent/main",

                        "http://localhost:8080/callback.html",
                        "http://localhost:8080/silent.html",
                        "http://localhost:8080/popup.html",

                        "https://ecng-profile.azurewebsites.net",
                        "https://ecng-profile.azurewebsites.net/auth/signinpop/main",
                        "https://ecng-profile.azurewebsites.net/auth/signinwin/main",
                        "https://ecng-profile.azurewebsites.net/auth/signinsilent/main",
                    },

                    PostLogoutRedirectUris = { "https://localhost:44331/index.html", "https://ecng-identity.azurewebsites.net" },
                    AllowedCorsOrigins = { " http://localhost:8080", "https://localhost:44339", "https://ecng-profile.azurewebsites.net" },
                    AllowAccessTokensViaBrowser = true,
                    AllowedScopes = { "openid", "profile", "transactions_api" }
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
                        "http://localhost:8080/",
                        "http://localhost:8080/auth/signinpop/main",
                        "http://localhost:8080/auth/signinwin/main",
                        "http://localhost:8080/auth/signinsilent/main",

                        "https://ecng-merchants.azurewebsites.net",
                        "https://ecng-merchants.azurewebsites.net/auth/signinpop/main",
                        "https://ecng-merchants.azurewebsites.net/auth/signinwin/main",
                        "https://ecng-merchants.azurewebsites.net/auth/signinsilent/main",
                    },

                    PostLogoutRedirectUris = { "https://localhost:44331/index.html", "https://ecng-identity.azurewebsites.net" },
                    AllowedCorsOrigins = { " http://localhost:8080", "https://localhost:44390", "https://ecng-merchants.azurewebsites.net" },
                    AllowAccessTokensViaBrowser = true,
                    AllowedScopes = { "openid", "profile", "transactions_api", "merchants_api" }
                },
                new Client
                {
                    ClientId = "management_api",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    ClientSecrets =
                    {
                        new Secret("yuwsCT8cbJVgw2W6".Sha256())
                    },
                    AllowedScopes = { "management_api", "merchants_api" },
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