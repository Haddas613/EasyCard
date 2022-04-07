using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Api.Models;
using Shared.Helpers.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers.WebHooks
{
    public class WebHookService : IWebHookService
    {
        private readonly IWebApiClient webApiClient;
        private readonly ILogger logger;
        private readonly IMetricsService metricsService;

        public WebHookService(IWebApiClient webApiClient, ILogger<WebHookService> logger, IMetricsService metricsService)
        {
            this.webApiClient = webApiClient;
            this.logger = logger;
            this.metricsService = metricsService;
        }

        public async Task<OperationResponse> ExecuteWebHook(WebHookData webHookData)
        {
            var payload = JsonConvert.SerializeObject(webHookData.Payload);
            var dimensions = new Dictionary<string, string>()
            {
                { nameof(webHookData.EventID), webHookData.EventID.ToString() },
                { nameof(webHookData.Url), webHookData.Url },
                { nameof(webHookData.CorrelationId), webHookData.CorrelationId },
                { nameof(webHookData.MerchantID), webHookData.MerchantID?.ToString() },
                { nameof(webHookData.TerminalID), webHookData.TerminalID?.ToString() },
                { nameof(webHookData.Payload), payload }
            };

            try
            {
                await webApiClient.PostRaw(webHookData.Url, null, payload, "application/json", () =>
                {
                    NameValueCollection headers = new NameValueCollection();
                    if (!string.IsNullOrWhiteSpace(webHookData.SecurityHeader?.Key))
                    {
                        headers.Add(webHookData.SecurityHeader.Key, webHookData.SecurityHeader.Value);
                    }

                    return Task.FromResult(headers);
                });

                metricsService.TrackEvent(
                    eventName: "ExecuteWebHook",
                    properties: dimensions,
                    metrics: null
                    );

                return new OperationResponse();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Failed to execute WebHook to {webHookData.Url}; CorrelationID: {webHookData.CorrelationId}: {ex.Message}");

                metricsService.TrackEvent(
                    eventName: "ExecuteWebHookFailed",
                    properties: dimensions,
                    metrics: null
                    );

                return new OperationResponse(ex.Message, Api.Models.Enums.StatusEnum.Error);
            }
        }
    }
}
