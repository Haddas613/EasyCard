using Ecwid.Api.Models;
using Ecwid.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Shared.Api.Models;
using Shared.Helpers;
using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ecwid.Api
{
    public class EcwidApiClient : IEcwidApiClient
    {
        private readonly IWebApiClient apiClient;
        private readonly IIntegrationRequestLogStorageService integrationRequestLogStorageService;
        private readonly ILogger logger;

        private readonly EcwidGlobalSettings ecwidGlobalSettings;

        public EcwidApiClient(
            IWebApiClient apiClient,
            IIntegrationRequestLogStorageService integrationRequestLogStorageService,
            ILogger logger,
            IOptions<EcwidGlobalSettings> ecwidGlobalSettings)
        {
            this.apiClient = apiClient;
            this.integrationRequestLogStorageService = integrationRequestLogStorageService;
            this.logger = logger;
            this.ecwidGlobalSettings = ecwidGlobalSettings.Value;
        }

        public async Task<OperationResponse> UpdateOrderStatus(EcwidUpdateOrderStatusRequest request)
        {
            EcwidUpdateStatusResponse result = null;
            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            try
            {
                var payload = new
                {
                    paymentStatus = request.Status.ToString(),
                };

                result = await apiClient.Put<EcwidUpdateStatusResponse>(ecwidGlobalSettings.ApiBaseAddress,
                    $"api/v3/{request.StoreID}/orders/{request.ReferenceTransactionID}?token={request.Token}", payload, null,
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

            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{nameof(EcwidApiClient)}.{nameof(UpdateOrderStatus)} Error: {ex.Message}");
                throw;
            }
            finally
            {
                IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, request.PaymentTransactionID.ToString(), integrationMessageId, request.CorrelationId);

                integrationMessage.Request = requestStr;
                integrationMessage.Response = responseStr;
                integrationMessage.ResponseStatus = responseStatusStr;
                integrationMessage.Address = requestUrl;

                await integrationRequestLogStorageService.Save(integrationMessage);
            }

            var response = new OperationResponse(responseStatusStr,
                result.UpdateCount > 0 ? Shared.Api.Models.Enums.StatusEnum.Success : Shared.Api.Models.Enums.StatusEnum.Error);

            return response;
        }
    }
}
