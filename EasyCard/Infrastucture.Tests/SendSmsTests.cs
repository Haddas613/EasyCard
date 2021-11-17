using BasicServices;
using InforU;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Api.Models.Enums;
using Shared.Helpers;
using Shared.Helpers.Services;
using Shared.Helpers.Sms;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Infrastucture.Tests
{
    public class SendSmsTests
    {
        [Fact(DisplayName = "Send Sms Using InforU")]
        public async Task SendSms()
        {
            var DefaultStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=ecng;AccountKey=4NjfZ4WcFlvNBzKHgbyGDdl+iYBiUv1SPU2hVneIqDyX0TsHUXtG707cfrGxnOCHD85L8mLRamck9w014/m1Vg==;EndpointSuffix=core.windows.net";

            //          "InforUMobileSmsSettings": {
            //              "InforUMobileSmsRequestsLogStorageTable": "inforusms",
            //  "InforUMobileBaseUrl": "https://uapi.inforu.co.il/",
            //  "UserName": "rapid101",
            //  "Password": "rapid202"
            //}

            var storageService = new IntegrationRequestLogStorageService(DefaultStorageConnectionString, "sms", null);
            var inforUMobileSmsSettings = new InforUMobileSmsSettings
            {
                InforUMobileBaseUrl = "https://uapi.inforu.co.il/",
                InforUMobileSmsRequestsLogStorageTable = "inforusms",
                UserName = "test",
                Password = "1234"
            };

            var webApiClient = new WebApiClient();

            var logger = new Mock<ILogger<InforUMobileSmsService>>();
            var metrics = new Mock<IMetricsService>();



            ISmsService smsService = new InforUMobileSmsService(webApiClient, inforUMobileSmsSettings, logger.Object, storageService, metrics.Object, false);




            var response = await smsService.Send(new SmsMessage
            {
                MerchantID = null,
                MessageId = Guid.NewGuid().ToString(),
                Body = "Test",
                From = "test",
                To = "1234",
                CorrelationId = Guid.NewGuid().ToString()
            });

            Assert.Equal(StatusEnum.Success, response.Status);
        }
    }
}
