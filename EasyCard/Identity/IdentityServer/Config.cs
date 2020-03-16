﻿using IdentityServer4.Models;
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
                new ApiResource("merchants_api", "Merchants Api"),
                new ApiResource("transactions_api", "Transactions Api"),
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
                // client credentials flow client
                new Client
                {
                    ClientId = "client",
                    ClientName = "Client Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("2tAT3USPEcqWhtcH".Sha256()) },

                    AllowedScopes = { "api1" }
                },

                // MVC client using code flow + pkce
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",

                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    RequirePkce = true,
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    RedirectUris = { "http://localhost:5003/signin-oidc" },
                    FrontChannelLogoutUri = "http://localhost:5003/signout-oidc",
                    PostLogoutRedirectUris = { "http://localhost:5003/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "api1" }
                },

                // SPA client using code flow + pkce
                new Client
                {
                    ClientId = "spa",
                    ClientName = "SPA Client",
                    ClientUri = "http://identityserver.io",

                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    RequirePkce = true,
                    RequireClientSecret = false,

                    RedirectUris =
                    {
                        "http://localhost:5002/index.html",
                        "http://localhost:5002/callback.html",
                        "http://localhost:5002/silent.html",
                        "http://localhost:5002/popup.html",
                    },

                    PostLogoutRedirectUris = { "http://localhost:5002/index.html" },
                    AllowedCorsOrigins = { "http://localhost:5002" },

                    AllowedScopes = { "openid", "profile", "merchants_api" }
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
                    AllowedScopes = { "management_api" },
                    AccessTokenType = AccessTokenType.Jwt,
                },
                new Client
                {
                    ClientId = "terminal",

                    RequireClientSecret = false,

                    //ClientSecrets = new List<Secret>
                    //{
                    //    new Secret("secret".Sha256())
                    //},

                    AllowedGrantTypes = { "my_crap_grant" },

                    AllowedScopes = new List<string>
                    {
                        "transactions_api"
                    }
                }
            };
    }
}