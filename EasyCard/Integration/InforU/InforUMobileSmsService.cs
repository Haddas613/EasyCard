﻿using Microsoft.Extensions.Logging;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.Sms;
using Shared.Integration;
using Shared.Integration.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        private readonly bool doNotSend;

        public InforUMobileSmsService(IWebApiClient apiClient, InforUMobileSmsSettings configuration, ILogger<InforUMobileSmsService> logger, IHttpContextAccessorWrapper httpContextAccessor, IIntegrationRequestLogStorageService storageService, bool doNotSend = false)
        {
            this.configuration = configuration;


            this.storageService = storageService;


            this.apiClient = apiClient;

            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.doNotSend = doNotSend;
        }

        public async Task<OperationResponse> Send(SmsMessage message)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);
            var correlationId = httpContextAccessor?.GetCorrelationId();
            IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.Now, integrationMessageId, correlationId);

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
                Content = new Content { Message = message.Body, Type = "sms" }
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
                svcResStr = await this.apiClient.PostRawForm(this.configuration.InforUMobileBaseUrl, "/SendMessageXml.ashx", dict, () => Task.FromResult(headers));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Exception {correlationId}: {ex.Message} ({message.MessageId})");

                await storageService.Save(integrationMessage);

                throw new IntegrationException("InforUMobileSms integration request failed", message.MessageId);
            }

            await storageService.Save(integrationMessage);

            if (svcResStr == null) throw new IntegrationException("InforUMobileSms integration request failed", message.MessageId);

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