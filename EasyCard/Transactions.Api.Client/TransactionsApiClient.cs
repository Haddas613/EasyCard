//using Microsoft.Extensions.Logging;
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
using Shared.Api.Configuration;
using Transactions.Api.Models.Billing;
using SharedApi = Shared.Api;

namespace Transactions.Api.Client
{
    public class TransactionsApiClient : ITransactionsApiClient
    {
        private readonly IWebApiClient webApiClient;
        private readonly ApiSettings apiConfiguration;
        //private readonly ILogger logger;
        private readonly IWebApiClientTokenService tokenService;

        public TransactionsApiClient(IWebApiClient webApiClient, /*ILogger logger,*/ IWebApiClientTokenService tokenService, IOptions<ApiSettings> apiConfiguration)
        {
            this.webApiClient = webApiClient;
            //this.logger = logger;
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
                //logger.LogError(clientError.Message);
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message, Status = SharedApi.Models.Enums.StatusEnum.Error });
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
                //logger.LogError(clientError.Message);
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message, Status = SharedApi.Models.Enums.StatusEnum.Error });
            }
        }

        public async Task<CheckoutData> GetCheckout(Guid? paymentRequestID, string apiKey, Guid? consumerID = null)
        {
            try
            {
                return await webApiClient.Get<CheckoutData>(apiConfiguration.TransactionsApiAddress, "api/checkout", new { paymentRequestID, apiKey, consumerID }, BuildHeaders);
            }
            catch (WebApiClientErrorException)
            {
                //var response = clientError.TryConvert(new OperationResponse { Message = clientError.Message });
                //logger.LogError($"Failed to get checkout data: {response?.Message}, correlationID: {response?.CorrelationId}");
                throw;
            }
        }

        public async Task<OperationResponse> GenerateInvoice(Guid? invoiceID)
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(apiConfiguration.TransactionsApiAddress, $"api/invoicing/generate/{invoiceID}", new { }, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                //logger.LogError(clientError.Message);
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message, Status = SharedApi.Models.Enums.StatusEnum.Error });
            }
        }

        public async Task<SummariesResponse<TransmitTransactionResponse>> TransmitTerminalTransactions(Guid? terminalID)
        {
            try
            {
                return await webApiClient.Post<SummariesResponse<TransmitTransactionResponse>>(apiConfiguration.TransactionsApiAddress, $"api/transmission/transmitByTerminal/{terminalID}", null, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                //logger.LogError(clientError.Message);
                return clientError.TryConvert<SummariesResponse<TransmitTransactionResponse>>();
            }
        }

        public async Task<CreateTransactionFromBillingDealsResponse> CreateTransactionsFromBillingDeals(CreateTransactionFromBillingDealsRequest request)
        {
            try
            {
                return await webApiClient.Post<CreateTransactionFromBillingDealsResponse>(apiConfiguration.TransactionsApiAddress, $"api/transactions/trigger-billing-deals", request, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                //logger.LogError(clientError.Message);
                return clientError.TryConvert(new CreateTransactionFromBillingDealsResponse { FailedCount = 1, Status = SharedApi.Models.Enums.StatusEnum.Error, Message = clientError.Message });
            }
        }

        public async Task<IEnumerable<Guid>> GetNonTransmittedTransactionsTerminals()
        {
            try
            {
                return await webApiClient.Get<IEnumerable<Guid>>(apiConfiguration.TransactionsApiAddress, $"api/transmission/nontransmittedtransactionterminals", null, BuildHeaders);
            }
            catch (WebApiClientErrorException)
            {
                throw;
            }
        }

        public async Task<SendBillingDealsToQueueResponse> SendBillingDealsToQueue()
        {
            try
            {
                return await webApiClient.Post<SendBillingDealsToQueueResponse>(apiConfiguration.TransactionsApiAddress, $"api/billing/due-billings", null, BuildHeaders);
            }
            catch (WebApiClientErrorException)
            {
                throw;
            }
        }

        public async Task<OperationResponse> CancelPaymentRequest(Guid paymentRequestID)
        {
            try
            {
                return await webApiClient.Delete<OperationResponse>(apiConfiguration.TransactionsApiAddress, $"api/paymentRequests/{paymentRequestID}", BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                //logger.LogError(clientError.Message);
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message, Status = SharedApi.Models.Enums.StatusEnum.Error });
            }
        }

        public async Task<OperationResponse> DeleteConsumerRelatedData(Guid consumerID)
        {
            try
            {
                return await webApiClient.Delete<OperationResponse>(apiConfiguration.TransactionsApiAddress, $"api/adminwebhook/deleteConsumerRelatedData/{consumerID}", BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                //logger.LogError(clientError.Message);
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message, Status = SharedApi.Models.Enums.StatusEnum.Error });
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
