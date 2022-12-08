using InforU.Extensions;
using Microsoft.Extensions.Logging;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Helpers;
using Shared.Helpers.Services;
using Shared.Helpers.Sms;
using Shared.Integration;
using Shared.Integration.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace InforU
{
    public class InforUMobileSmsService : ISmsService
    {

        private readonly IIntegrationRequestLogStorageService storageService;
        private readonly IWebApiClient apiClient;
        private readonly InforUMobileSmsSettings configuration;
        private readonly ILogger logger;
        private readonly IMetricsService metrics;

        private readonly bool doNotSend;

        public InforUMobileSmsService(IWebApiClient apiClient,
            InforUMobileSmsSettings configuration,
            ILogger<InforUMobileSmsService> logger,
            IIntegrationRequestLogStorageService storageService,
            IMetricsService metrics,
            bool doNotSend = false)
        {
            this.configuration = configuration;
            this.storageService = storageService;
            this.apiClient = apiClient;
            this.metrics = metrics;
            this.logger = logger;
            this.doNotSend = doNotSend;
        }

        public async Task<OperationResponse> Send(SmsMessage message)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);
            var correlationId = message.CorrelationId;
            IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.Now, message.To, integrationMessageId, correlationId);

            integrationMessage.MessageId = message.MessageId;
            integrationMessage.MerchantID = message.MerchantID;
            integrationMessage.TerminalID = message.TerminalID;
            integrationMessage.MessageDate = DateTime.UtcNow;

            NameValueCollection headers = new NameValueCollection();

            //headers.Add("SOAPAction", $"{soapAction}");

            var xml = new Inforu
            {
                User = new User { Username = this.configuration.UserName, Password = this.configuration.Password },
                Recipients = new Recipients { PhoneNumber = message.To },
                Content = new Content { Message = message.Body, Type = "sms" },
                Settings = new Settings { Sender = message.From }
            };

            var xmlStr = xml.ToXml();

            integrationMessage.Request = xmlStr;

            if (this.doNotSend)
            {
                await storageService.Save(integrationMessage);

                return new OperationResponse() { Status = StatusEnum.Success, Message = "Ok" };
            }

            SendSmsResponse svcRes = null;
            string svcResStr = null;

            var dict = new Dictionary<string, string>();
            dict.Add("InforuXML", xmlStr);

            try
            {
                svcResStr = await this.apiClient.PostRawForm(this.configuration.InforUMobileBaseUrl, "/SendMessageXml.ashx", dict, () => Task.FromResult(headers), 
                    onResponse: (string response, HttpStatusCode responseStatus, HttpResponseHeaders responseHeaders) => {
                        integrationMessage.ResponseStatus = responseStatus.ToString();
                    });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Exception {correlationId}: {ex.Message} ({message.MessageId})");

                await storageService.Save(integrationMessage);

                _ = Task.Run(() => metrics.TrackSmsEvent(false, message));
                throw new IntegrationException("InforUMobileSms integration request failed", message.MessageId);
            }

            integrationMessage.Response = svcResStr;
            await storageService.Save(integrationMessage);

            if (svcResStr == null) {
                _ = Task.Run(() => metrics.TrackSmsEvent(false, message));
                throw new IntegrationException("InforUMobileSms integration request failed", message.MessageId);
            }

            _ = Task.Run(() => metrics.TrackSmsEvent(true, message));
            try
            {
                svcRes = svcResStr.FromXml<SendSmsResponse>();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Exception {correlationId}: {ex.Message} ({message.MessageId})");

                throw new IntegrationException("InforUMobileSms integration request failed", message.MessageId);
            }

            var response = new OperationResponse() { Status = StatusEnum.Success, Message = svcRes.Description };

            if (svcRes.Status != "1")
            {
                response.Status = StatusEnum.Error;
            }

            return response;
        }
    }
}
