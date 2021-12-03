using Bit.Configuration;
using Bit.Converters;
using Bit.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Helpers;
using Shared.Helpers.Security;
using Shared.Integration;
using Shared.Integration.Exceptions;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Bit
{
    public class BitProcessor : IProcessor
    {
        private readonly IWebApiClient apiClient;
        private readonly IWebApiClientTokenService tokenService;
        private readonly ILogger logger;
        private readonly IIntegrationRequestLogStorageService integrationRequestLogStorageService;
        private readonly BitGlobalSettings configuration;

        public BitProcessor(IOptions<BitGlobalSettings> configuration,
            IWebApiClient apiClient,
            ILogger<BitProcessor> logger,
            IIntegrationRequestLogStorageService integrationRequestLogStorageService,
            IWebApiClientTokenService tokenService)
        {
            this.configuration = configuration.Value;
            this.apiClient = apiClient;
            this.logger = logger;
            this.integrationRequestLogStorageService = integrationRequestLogStorageService;
            this.tokenService = tokenService;
        }

        public async Task<ProcessorCreateTransactionResponse> CreateTransaction(ProcessorCreateTransactionRequest paymentTransactionRequest)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            var bitTransaction = await GetBitTransaction(paymentTransactionRequest.BitPaymentInitiationId, integrationMessageId);

            ValidateAgainstBitTransaction(paymentTransactionRequest, bitTransaction, integrationMessageId);

            var captureRequest = new BitCaptureRequest
            {
                RequestAmount = bitTransaction.RequestAmount,
                PaymentInitiationId = bitTransaction.PaymentInitiationId,
                SourceTransactionId = bitTransaction.SourceTransactionId,
                IssuerTransactionId =  bitTransaction.IssuerTransactionId
            };

            var captureResponse = await CaptureBitTransaction(captureRequest, paymentTransactionRequest.PaymentTransactionID, integrationMessageId, paymentTransactionRequest.CorrelationId);

            return captureResponse.GetBitCreateTransactionResponse();
        }

        public Task<IEnumerable<IntegrationMessage>> GetStorageLogs(string entityID)
        {
            return integrationRequestLogStorageService.GetAll(entityID);
        }

        // do not implement
        public Task ParamsUpdateTransaction(ProcessorUpdateParametersRequest updateParametersRequest)
        {
            throw new NotImplementedException();
        }

        // do not implement
        public Task<ProcessorPreCreateTransactionResponse> PreCreateTransaction(ProcessorCreateTransactionRequest paymentTransactionRequest)
        {
            throw new NotImplementedException();
        }

        // do not implement
        public Task<ProcessorTransmitTransactionsResponse> TransmitTransactions(ProcessorTransmitTransactionsRequest transmitTransactionsRequest)
        {
            throw new NotImplementedException();
        }

        private string GetBitTransactionUrl(string paymentInitiationId)
        {
            return $"/payments/bit/v2/single-payments/{paymentInitiationId}";
        }

        private async Task<BitTransactionResponse> GetBitTransaction(string paymentInitiationId, string integrationMessageId)
        {
            try
            {
                return await apiClient.Get<BitTransactionResponse>(configuration.BaseUrl, GetBitTransactionUrl(paymentInitiationId), null, BuildHeaders);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Bit integration request failed ({integrationMessageId}): {ex.Message}");

                throw new IntegrationException("Bit integration request failed", integrationMessageId);
            }
        }

        private async Task<BitTransactionResponse> DeleteBitTransaction(string paymentInitiationId, string integrationMessageId)
        {
            try
            {
                return await apiClient.Delete<BitTransactionResponse>(configuration.BaseUrl, GetBitTransactionUrl(paymentInitiationId), BuildHeaders);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Bit integration request failed ({integrationMessageId}): {ex.Message}");

                throw new IntegrationException("Bit integration request failed", integrationMessageId);
            }
        }

        private async Task<BitCaptureResponse> CaptureBitTransaction(BitCaptureRequest request, string paymentTransactionID, string integrationMessageId, string correlationID)
        {
            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;

            try
            {
                var response =  await apiClient.Post<BitCaptureResponse>(configuration.BaseUrl, $"{GetBitTransactionUrl(request.PaymentInitiationId)}/capture", request, BuildHeaders,
                    (url, request) =>
                    {
                        requestStr = request;
                        requestUrl = url;
                    },
                    (response, responseStatus, responseHeaders) =>
                    {
                        responseStr = response;
                        responseStatusStr = responseStatus.ToString();
                    });

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Bit integration request failed ({integrationMessageId}): {ex.Message}");

                throw new IntegrationException("Bit integration request failed", integrationMessageId);
            }
            finally
            {
                IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, paymentTransactionID, integrationMessageId, correlationID);

                integrationMessage.Request = requestStr;
                integrationMessage.Response = responseStr;
                integrationMessage.ResponseStatus = responseStatusStr;
                integrationMessage.Address = requestUrl;

                await integrationRequestLogStorageService.Save(integrationMessage);
            }
        }

        private void ValidateAgainstBitTransaction(ProcessorCreateTransactionRequest processorRequest, BitTransactionResponse bitTransaction, string integrationMessageId)
        {
            if (processorRequest.TransactionAmount != bitTransaction.RequestAmount)
            {
                throw new BusinessException("Bit integration validation failed failed. Amount mismatch");
            }
        }

        private async Task<NameValueCollection> BuildHeaders()
        {
            NameValueCollection headers = new NameValueCollection();
            headers.Add("Ocp-Apim-Subscription-Key", configuration.OcpApimSubscriptionKey);

            var apiToken = await tokenService.GetToken(headers);

            if (apiToken != null)
            {
                headers.Add("Authorization", new AuthenticationHeaderValue("Bearer", apiToken.AccessToken).ToString());
            }

            return headers;
        }
    }
}
