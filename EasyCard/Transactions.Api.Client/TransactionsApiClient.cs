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
using Transactions.Api.Models.UpdateParameters;
using Transactions.Api.Models.Currency;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.PaymentRequests;
using Transactions.Api.Models.External.Bit;
using Bit.Models;
using Transactions.Api.Models.Invoicing;
using Transactions.Api.Models.External.ThreeDS;

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

        public NameValueCollection Headers { get; } = new NameValueCollection();

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

        public async Task<CheckoutData> GetCheckout(Guid? paymentRequestID, Guid? paymentIntentID, string apiKey)
        {
            try
            {
                return await webApiClient.Get<CheckoutData>(apiConfiguration.TransactionsApiAddress, "api/checkout", new { paymentRequestID, paymentIntentID, apiKey }, BuildHeaders);
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

        public async Task<OperationResponse> TransmitTerminalTransactions(Guid? terminalID)
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(apiConfiguration.TransactionsApiAddress, $"api/transmission/transmitByTerminal/{terminalID}", null, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                //logger.LogError(clientError.Message);
                return clientError.TryConvert<OperationResponse>();
            }
        }

        public async Task<CreateTransactionFromBillingDealsResponse> CreateTransactionsFromBillingDeals(CreateTransactionFromBillingDealsRequest request)
        {
            try
            {
                return await webApiClient.Post<CreateTransactionFromBillingDealsResponse>(apiConfiguration.TransactionsApiAddress, $"api/transactions/process-billing-deals", request, BuildHeaders);
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

        public async Task<SummariesResponse<TransmitTransactionResponse>> TransmitTransactions(TransmitTransactionsRequest request)
        {
            try
            {
                return await webApiClient.Post<SummariesResponse<TransmitTransactionResponse>>(apiConfiguration.TransactionsApiAddress, $"api/transmission/transmit", request, BuildHeaders);
            }
            catch (WebApiClientErrorException)
            {
                throw;
            }
        }

        public async Task<SendBillingDealsToQueueResponse> SendBillingDealsToQueue(Guid terminalID)
        {
            try
            {
                return await webApiClient.Post<SendBillingDealsToQueueResponse>(apiConfiguration.TransactionsApiAddress, $"api/billing/due-billings/{terminalID}", null, BuildHeaders);
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

        public async Task<OperationResponse> CreatePaymentIntent(PaymentRequestCreate model)
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(apiConfiguration.TransactionsApiAddress, "api/paymentIntent", model, BuildHeaders);
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

        public async Task<TransactionResponseAdmin> GetTransaction(Guid? transactionID)
        {
            try
            {
                return await webApiClient.Get<TransactionResponseAdmin>(apiConfiguration.TransactionsApiAddress, $"api/transactions/{transactionID}", null, BuildHeaders);
            }
            catch (WebApiClientErrorException)
            {
                throw;
            }
        }

        public async Task<OperationResponse> GenerateMasavFile()
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(apiConfiguration.TransactionsApiAddress, $"api/masav/generate/", new { }, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                //logger.LogError(clientError.Message);
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message, Status = SharedApi.Models.Enums.StatusEnum.Error });
            }
        }

        public async Task<OperationResponse> UpdateCurrencyRates(CurrencyRateUpdateRequest request)
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(apiConfiguration.TransactionsApiAddress, $"api/currency", request, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message, Status = SharedApi.Models.Enums.StatusEnum.Error });
            }
        }

        public async Task<TransactionResponse> GetTransaction(string transactionID)
        {
            var transaction = await webApiClient.Get<TransactionResponse>(apiConfiguration.TransactionsApiAddress, $"/api/transactions/{transactionID}", null, BuildHeaders);

            return transaction;
        }

        public async Task<SummariesResponse<BillingDealSummary>> GetBillingDeals(BillingDealsFilter filter)
        {
            var transaction = await webApiClient.Get<SummariesResponse<BillingDealSummary>>(apiConfiguration.TransactionsApiAddress, $"/api/billing", filter, BuildHeaders);

            return transaction;
        }

        public async Task<BillingDealResponse> GetBillingDeal(Guid billingDealID)
        {
            var transaction = await webApiClient.Get<BillingDealResponse>(apiConfiguration.TransactionsApiAddress, $"/api/billing/{billingDealID}", null, BuildHeaders);

            return transaction;
        }

        public async Task<OperationResponse> CreateBillingDeal(BillingDealRequest model)
        {
            var res = await webApiClient.Post<OperationResponse>(apiConfiguration.TransactionsApiAddress, $"/api/billing", model, BuildHeaders);

            return res;
        }

        public async Task<OperationResponse> PauseBillingDeal(Guid billingDealID, PauseBillingDealRequest model)
        {
            var res = await webApiClient.Post<OperationResponse>(apiConfiguration.TransactionsApiAddress, $"/api/billing/{billingDealID}/pause", model, BuildHeaders);
            return res;
        }

        public async Task<OperationResponse> UpdateBillingDeal(Guid billingDealID, BillingDealUpdateRequest model)
        {
            var res = await webApiClient.Put<OperationResponse>(apiConfiguration.TransactionsApiAddress, $"/api/billing/{billingDealID}", model, BuildHeaders);

            return res;
        }

        public async Task<OperationResponse> CreateBillingDealInvoiceOnly(BillingDealInvoiceOnlyRequest model)
        {
            var res = await webApiClient.Post<OperationResponse>(apiConfiguration.TransactionsApiAddress, $"/api/invoiceonlybilling", model, BuildHeaders);

            return res;
        }

        public async Task<OperationResponse> UpdateBillingDealInvoiceOnly(Guid billingDealID, BillingDealInvoiceOnlyUpdateRequest model)
        {
            var res = await webApiClient.Put<OperationResponse>(apiConfiguration.TransactionsApiAddress, $"/api/invoiceonlybilling/{billingDealID}", model, BuildHeaders);

            return res;
        }

        // It should be reworked to enable/disable methods pair
        [Obsolete]
        public async Task<OperationResponse> SwitchBillingDeal(Guid billingDealID)
        {
            var res = await webApiClient.Post<OperationResponse>(apiConfiguration.TransactionsApiAddress, $"/api/billing/{billingDealID}/switch", null, BuildHeaders);

            return res;
        }

        public async Task<SummariesResponse<CreditCardTokenSummary>> GetTokens(CreditCardTokenFilter filter)
        {
            var res = await webApiClient.Get<SummariesResponse<CreditCardTokenSummary>>(apiConfiguration.TransactionsApiAddress, $"/api/cardtokens", filter, BuildHeaders);

            return res;
        }

        public async Task<OperationResponse> CreateToken(TokenRequest model)
        {
            var res = await webApiClient.Post<OperationResponse>(apiConfiguration.TransactionsApiAddress, $"/api/cardtokens", model, BuildHeaders);

            return res;
        }

        public async Task<OperationResponse> DeleteToken(string key)
        {
            var res = await webApiClient.Delete<OperationResponse>(apiConfiguration.TransactionsApiAddress, $"/api/cardtokens/{key}", BuildHeaders);

            return res;
        }

        private async Task<NameValueCollection> BuildHeaders()
        {
            var token = await tokenService.GetToken();

            NameValueCollection headers = new NameValueCollection(Headers);

            if (token != null)
            {
                headers.Add("Authorization", new AuthenticationHeaderValue("Bearer", token.AccessToken).ToString());
            }

            return headers;
        }

        public async Task<OperationResponse> CreateInvoiceForTransaction(Guid transactionID)
        {
            var res = await webApiClient.Post<OperationResponse>(apiConfiguration.TransactionsApiAddress, $"/api/invoicing/transaction/{transactionID}", null, BuildHeaders);

            return res;
        }

        public async Task<SummariesAmountResponse<TransactionSummaryAdmin>> GetTransactions(TransactionsFilter filter)
        {
            return await webApiClient.Get<SummariesAmountResponse<TransactionSummaryAdmin>>(apiConfiguration.TransactionsApiAddress, $"api/transactions", filter, BuildHeaders);
        }

        public async Task<InitialBitOperationResponse> InitiateBitTransaction(CreateTransactionRequest model)
        {
            try
            {
                return await webApiClient.Post<InitialBitOperationResponse>(apiConfiguration.TransactionsApiAddress, "api/external/bit/initial", model, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                //logger.LogError(clientError.Message);
                return clientError.TryConvert(new InitialBitOperationResponse { Message = clientError.Message, Status = SharedApi.Models.Enums.StatusEnum.Error });
            }
        }

        public async Task<OperationResponse> CaptureBitTransaction(CaptureBitTransactionRequest model)
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(apiConfiguration.TransactionsApiAddress, "api/external/bit/capture", model, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                //logger.LogError(clientError.Message);
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message, Status = SharedApi.Models.Enums.StatusEnum.Error });
            }
        }

        public async Task<OperationResponse> BitTransactionPostProcessing(Guid? transactionID)
        {
            try
            {
                return await webApiClient.Post<OperationResponse>(apiConfiguration.TransactionsApiAddress, $"api/external/bit/cancelOrConfirmPending?transactionID={transactionID}", null, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                return clientError.TryConvert(new OperationResponse { Message = clientError.Message, Status = SharedApi.Models.Enums.StatusEnum.Error });
            }
        }

        public async Task<BitTransactionResponse> GetBitTransaction(GetBitTransactionQuery request)
        {
            try
            {
                return await webApiClient.Get<BitTransactionResponse>(apiConfiguration.TransactionsApiAddress, "api/external/bit/get", request, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                throw;
                //logger.LogError(clientError.Message);
                //return null;
            }
        }

        public async Task<CreateInvoicingConsumerResponse> CreateInvoicingConsumer(CreateInvoicingConsumerRequest consumerRequest)
        {
            var res = await webApiClient.Post<CreateInvoicingConsumerResponse>(apiConfiguration.TransactionsApiAddress, $"/api/invoicing/createConsumer", consumerRequest, BuildHeaders);

            return res;
        }

        public async Task<Versioning3DsResponse> Versioning3Ds(Versioning3DsRequest request)
        {
            try
            {
                return await webApiClient.Post<Versioning3DsResponse>(apiConfiguration.TransactionsApiAddress, $"api/external/3ds/versioning", request, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                throw;
            }
        }
    }
}
