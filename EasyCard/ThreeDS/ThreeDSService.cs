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
        public async Task<VersioningResponseEnvelop> Versioning(ThreeDS.Contract.Versioning3DsRequestModel model, string correlationID)
        {
            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            VersioningRequest request = new VersioningRequest()
            {
                CardNumber = model.CardNumber,
                UserName = configuration.UserName,
                Password = configuration.Password,
                PspID = configuration.PspID,
                // NotificationURL = model.NotificationURL
            };

            try
            {
                var response = await apiClient.Post<VersioningResponseEnvelop>(configuration.BaseUrl, "api/Versioning", request, null,
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

                integrationMessage.Request = RemoveCardNumber(requestStr, model.CardNumber);
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
                MerchantURL = configuration.MerchantURL,
                MessageType = "ARes",

                AcctType = "02",
                AcquirerBin = "",
                AcquirerMerchantId = "",
                Brand = "",
                Merchant_mcc = "1234",
                PurchaseExponent = "2",
                TransType = "01",

                //CardExpiryDate = "2408",

                CardExpiryDateMonth = (model.CardExpiration?.Month).GetValueOrDefault().ToString("D2"),
                CardExpiryDateYear = (model.CardExpiration?.Year).GetValueOrDefault().ToString("D2"),

                MerchantName = model.MerchantName,

                PurchaseCurrency = CurrencyHelper.GetCurrencyISONumber(model.Currency).ToString(),
                PurchaseDate = DateTime.Today.ToString("yyyyMMddHHmmss"),
                PurchaseAmount = model.Amount == null ? null : (int?)(model.Amount.GetValueOrDefault() * 100),

                BrowserAcceptHeader = model.BrowserDetails?.BrowserAcceptHeader ?? "text/html,application/xhtml+xml,application/xml;",
                BrowserLanguage = model.BrowserDetails?.BrowserLanguage ?? "en",
                BrowserColorDepth = model.BrowserDetails?.BrowserColorDepth ?? "8", 
                BrowserScreenHeight = model.BrowserDetails?.BrowserScreenHeight ?? "1050", 
                BrowserScreenWidth = model.BrowserDetails?.BrowserScreenWidth ?? "1680", 
                BrowserTZ = model.BrowserDetails?.BrowserTZ ?? "1200", 
                BrowserUserAgent = model.BrowserDetails?.BrowserUserAgent ?? "Mozilla/5.0 (Windows NT 6.1; Win64; x64;",

                ChallengeWindowSize = "02"
            };

            try
            {
                var response = await apiClient.Post<AuthenticationResponseEnvelop>(configuration.BaseUrl, "api/authentication", request, null,
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

                integrationMessage.Request = RemoveCardNumber(requestStr, model.CardNumber);
                integrationMessage.Response = responseStr;
                integrationMessage.ResponseStatus = responseStatusStr;
                integrationMessage.Address = requestUrl;

                await integrationRequestLogStorageService.Save(integrationMessage);
            }
        }

        private static string RemoveCardNumber(string content, string cardNumber)
        {
            return content.Replace(cardNumber, CreditCardHelpers.GetCardDigits(cardNumber));
        }
    }
}
