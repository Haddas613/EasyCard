using MerchantProfileApi.Client;
using Microsoft.Extensions.Options;
using Shared.Api.Configuration;
using Shared.Helpers;
using Shared.Helpers.Security;
using Transactions.Api.Client;

namespace CheckoutPortal.Services
{
    public class ApiClientsFactory : IApiClientsFactory
    {
        private readonly ApiSettings apiSettings;

        private readonly WebApiClient webApiClient = new WebApiClient();

        public ApiClientsFactory(IOptions<ApiSettings> apiSettings)
        {
            this.apiSettings = apiSettings.Value;
        }

        public ITransactionsApiClient GetTransactionsApiClient(IWebApiClientTokenService tokenService)
        {
            return new TransactionsApiClient(webApiClient, tokenService, Options.Create(apiSettings));
        }

        public MerchantMetadataApiClient GetMerchantMetadataApiClient(IWebApiClientTokenService tokenService)
        {
            return new MerchantMetadataApiClient(webApiClient, tokenService, Options.Create(apiSettings));
        }
    }
}
