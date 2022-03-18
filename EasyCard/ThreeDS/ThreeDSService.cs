﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api.Models;
using Shared.Helpers;
using Shared.Integration;
using System;
using System.Threading.Tasks;
using ThreeDS.Configuration;
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
        public async Task<VersioningRestResponse> Versioning(string cardNumber)
        {
            string requestUrl = null;
            string requestStr = null;
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);
            VersioningRequest request = new VersioningRequest()
            {
                CardNumber = cardNumber,
                username = configuration.UserName,
                Password = configuration.Password,
                PspID = configuration.PspID
            };
            var response = await apiClient.Post<VersioningRestResponse>(configuration.BaseUrl, "/CreditCartPay/api/Versioning", request, null,
                (url, request) =>
                {
                    requestStr = request;
                    requestUrl = url;
                });
            return response;
        }


        public async Task<AuthenticateResponse> Authentication(AuthenticateReqModel request)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);
            AuthenticationRequest requestAuthen = new AuthenticationRequest()
            {
                UserName = configuration.UserName,
                Password = configuration.Password,
                PspID = configuration.PspID,
                Retailer = request.Retailer,
                threeDSServerTransID = request.threeDSServerTransID
            };
            var response = await apiClient.Post<AuthenticateResponse>(configuration.BaseUrl, "/CreditCartPay/api/authentication", requestAuthen
                  );
            return response;
        }


    }
}
