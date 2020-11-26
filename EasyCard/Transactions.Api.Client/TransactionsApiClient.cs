using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Shared.Helpers.Security;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using Shared.Helpers;
using Transactions.Api.Models.Transactions;
using Shared.Api.Models;
using Transactions.Api.Models.Checkout;

namespace Transactions.Api.Client
{
    public class TransactionsApiClient : ITransactionsApiClient
    {
        private readonly IWebApiClient webApiClient;
        private readonly TransactionsApiClientConfig apiConfiguration;
        private readonly ILogger logger;
        private readonly IWebApiClientTokenService tokenService;

        public TransactionsApiClient(IWebApiClient webApiClient, ILogger<TransactionsApiClient> logger, IWebApiClientTokenService tokenService, IOptions<TransactionsApiClientConfig> apiConfiguration)
        {
            this.webApiClient = webApiClient;
            this.logger = logger;
            this.apiConfiguration = apiConfiguration.Value;
            this.tokenService = tokenService;
        }

        public async Task<OperationResponse> CreateTransaction(CreateTransactionRequest model)
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(apiConfiguration.TransactionsApiAddress, "api/transactions/create", model, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError(clientError.Message);
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message });
            }
        }

        public async Task<OperationResponse> CreateTransactionPR(PRCreateTransactionRequest model)
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(apiConfiguration.TransactionsApiAddress, "api/transactions/prcreate", model, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError(clientError.Message);
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message });
            }
        }

        public async Task<CheckoutData> GetCheckout(Guid? paymentRequestID, string apiKey)
        {
            try
            {
                return await webApiClient.Get<CheckoutData>(apiConfiguration.TransactionsApiAddress, "api/checkout", new { paymentRequestID, apiKey}, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                var response = clientError.TryConvert(new OperationResponse { Message = clientError.Message });
                logger.LogError($"Failed to get checkout data: {response.Message}, correlationID: {response.CorrelationId}");
                throw;
            }
        }

        private async Task<NameValueCollection> BuildHeaders()
        {
            var token = await tokenService.GetToken();

            NameValueCollection headers = new NameValueCollection();

            if (token != null)
            {
                headers.Add("Authorization", new AuthenticationHeaderValue("Bearer", token.AccessToken).ToString());
            }

            return headers;
        }
    }
}
