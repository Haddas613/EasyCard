using MerchantProfileApi.Client;
using Microsoft.Extensions.Options;
using Shared.Api.Configuration;
using Shared.Helpers;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Api.Client;

namespace DesktopEasyCardConvertorECNG
{
    public class ServiceFactory
    {
        private readonly Configuration configuration;

        private readonly WebApiClient webApiClient = new WebApiClient();


        private static IDictionary<Environment, Configuration> configurations = new Dictionary<Environment, Configuration>()
        {
            {
                Environment.QA,
                new Configuration
                {
                    IdentityApiAddress = "https://ecng-identity.azurewebsites.net",
                    MetadataApiAddress = "https://ecng-profile.azurewebsites.net",
                    TransactionApiAddress = "https://ecng-transactions.azurewebsites.net",
                    CheckoutAddress = "https://ecng-checkout.azurewebsites.net"
                }
            },
            {
                Environment.STAGE,
                new Configuration
                {
                    IdentityApiAddress = "https://identity-stage.e-c.co.il",
                    MetadataApiAddress = "https://merchant-stage.e-c.co.il",
                    TransactionApiAddress = "https://api-stage.e-c.co.il",
                    CheckoutAddress = "https://checkout-stage.e-c.co.il"
                }
            },
            {
                Environment.LIVE,
                new Configuration
                {
                    IdentityApiAddress = "https://identity.e-c.co.il",
                    MetadataApiAddress = "https://merchant.e-c.co.il",
                    TransactionApiAddress = "https://api.e-c.co.il",
                    CheckoutAddress = "https://checkout.e-c.co.il"
                }
            },
            {
                Environment.DEV,
                new Configuration
                {
                    IdentityApiAddress = "https://ecng-identity.azurewebsites.net",
                    MetadataApiAddress = "https://localhost:44339",
                    TransactionApiAddress = "https://localhost:44322",
                    CheckoutAddress = "https://ecng-checkout.azurewebsites.net"
                }
            }
        };

        public ServiceFactory(Environment environment)
        {
            configuration = configurations[environment];
        }

        public TransactionsApiClient GetTransactionsApiClient(string privateKey)
        {
            var apiSettings = new ApiSettings
            {
                TransactionsApiAddress = configuration.TransactionApiAddress
            };

            var tokenService = new TokensService(privateKey, webApiClient, configuration);

            return new TransactionsApiClient(webApiClient, tokenService, Options.Create(apiSettings));
        }

        public MerchantMetadataApiClient GetMerchantMetadataApiClient(string privateKey)
        {
            var apiSettings = new ApiSettings
            {
                MerchantProfileURL = configuration.MetadataApiAddress
            };

            var tokenService = new TokensService(privateKey, webApiClient, configuration);

            return new MerchantMetadataApiClient(webApiClient, tokenService, Options.Create(apiSettings));
        }
    }
}
