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

namespace Transactions.Api.Client
{
    public class TransactionsApiClient : ITransactionsApiClient
    {
        private readonly IWebApiClient webApiClient;
        private readonly IdentityServerClientSettings configuration;
        private readonly ILogger logger;
        private readonly IWebApiClientTokenService tokenService;

        public TransactionsApiClient(IWebApiClient webApiClient, ILogger<TransactionsApiClient> logger, IOptions<IdentityServerClientSettings> configuration, IWebApiClientTokenService tokenService)
        {
            this.webApiClient = webApiClient;
            this.logger = logger;
            this.configuration = configuration.Value;
            this.tokenService = tokenService;
        }

        public async Task<OperationResponse> CreateTransaction(CreateTransactionRequest model)
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(configuration.Authority, "api/transactions/create", model, BuildHeaders);
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
                return await webApiClient.Post<OperationResponse>(configuration.Authority, "api/transactions/prcreate", model, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError(clientError.Message);
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message });
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
