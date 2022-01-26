using MerchantProfileApi.Client;
using Shared.Helpers.Security;
using Transactions.Api.Client;

namespace CheckoutPortal.Services
{
    public interface IApiClientsFactory
    {
        ITransactionsApiClient GetTransactionsApiClient(IWebApiClientTokenService tokenService);

        MerchantMetadataApiClient GetMerchantMetadataApiClient(IWebApiClientTokenService tokenService);
    }
}
