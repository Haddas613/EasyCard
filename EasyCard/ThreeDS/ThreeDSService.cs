using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api.Models;
using Shared.Helpers;
using Shared.Integration;
using Shared.Integration.Exceptions;
using System;
using System.Threading.Tasks;
using ThreeDS.Configuration;
using ThreeDS.Contract;
using ThreeDS.Models;

namespace ThreeDS
{
    public class ThreeDSService
    {
        private readonly IWebApiClient apiClient;
        private readonly ILogger logger;
        private readonly IIntegrationRequestLogStorageService integrationRequestLogStorageService;
        private readonly ThreedDSGlobalConfiguration configuration;
        public ThreeDSService(IOptions<ThreedDSGlobalConfiguration> configuration,
            IWebApiClient apiClient,
            ILogger<ThreeDSService> logger,
            IIntegrationRequestLogStorageService integrationRequestLogStorageService
           )
        {
            this.configuration = configuration.Value;
            this.apiClient = apiClient;
            this.logger = logger;
            this.integrationRequestLogStorageService = integrationRequestLogStorageService;
        }
        public async Task<VersioningResponseEnvelop> Versioning(string cardNumber, string correlationID)
        {
            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            VersioningRequest request = new VersioningRequest()
            {
                CardNumber = cardNumber,
                UserName = configuration.UserName,
                Password = configuration.Password,
                PspID = configuration.PspID,
                NotificationURL = "https://localhost:44396/Home/Notification3Ds"
            };

            try
            {
                var response = await apiClient.Post<VersioningResponseEnvelop>(configuration.BaseUrl, "/CreditCartPay/api/Versioning", request, null,
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
                logger.LogError(ex, $"3DSecure integration request failed ({integrationMessageId}): {ex.Message}");

                throw new IntegrationException("3DSecure integration request failed", integrationMessageId);
            }
            finally
            {
                IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, integrationMessageId, integrationMessageId, correlationID);

                integrationMessage.Request = requestStr;
                integrationMessage.Response = responseStr;
                integrationMessage.ResponseStatus = responseStatusStr;
                integrationMessage.Address = requestUrl;

                await integrationRequestLogStorageService.Save(integrationMessage);
            }
        }

        public async Task<AuthenticationResponseEnvelop> Authentication(Authenticate3DsRequestModel model, string correlationID)
        {
            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            AuthenticationRequest request = new AuthenticationRequest()
            {
                UserName = configuration.UserName,
                Password = configuration.Password,
                PspID = configuration.PspID,
                Retailer = model.MerchantNumber,
                ThreeDSServerTransID = model.ThreeDSServerTransID,
                AcctNumber = model.CardNumber,
                NotificationURL = model.NotificationURL,
                MerchantURL = "https://https://localhost:44300/Home/Notification3Ds",
                AcctType = "02",
                AcquirerBin = "2",
                AcquirerMerchantId = "1",
                Brand = "2",
                MessageType = "ARes",
                PurchaseCurrency = CurrencyHelper.GetCurrencyISONumber(model.Currency).ToString(),
                PurchaseDate = DateTime.Today.ToString("yyyyMMddHHmmss"),
                Merchant_mcc = "1234",
                MerchantName = "Test",
                BrowserAcceptHeader = "text/html,application/xhtml+xml,application/xml;",
                BrowserLanguage = "en",
                BrowserColorDepth = "8", 
                BrowserScreenHeight = "1050", 
                BrowserScreenWidth = "1680", 
                BrowserTZ = "1200", 
                BrowserUserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64;"
            };

            try
            {
                var response = await apiClient.Post<AuthenticationResponseEnvelop>(configuration.BaseUrl, "/CreditCartPay/api/authentication", request, null,
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
                logger.LogError(ex, $"3DSecure integration request failed ({integrationMessageId}): {ex.Message}");

                throw new IntegrationException("3DSecure integration request failed", integrationMessageId);
            }
            finally
            {
                IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, integrationMessageId, integrationMessageId, correlationID);

                integrationMessage.Request = requestStr;
                integrationMessage.Response = responseStr;
                integrationMessage.ResponseStatus = responseStatusStr;
                integrationMessage.Address = requestUrl;

                await integrationRequestLogStorageService.Save(integrationMessage);
            }
        }
    }
}
