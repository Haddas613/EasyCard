using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        public async Task<VersioningRestResponse> Versioning( string cardNumber)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);
            VersioningRequest request = new VersioningRequest()
            {
                CardNumber = cardNumber,
                username = configuration.UserName,
                Password = configuration.Password,
                PspID = configuration.PspID
            };
            var response = await apiClient.Post<VersioningRestResponse>(configuration.BaseUrl, "versioning", request
                  );
            return response;
        }


        public async Task<VersioningRestResponse> Authentication()
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);
            AuthenticationRequest request = new AuthenticationRequest()
                {
                  UserName = configuration.UserName,
                  Password = configuration.Password,
                  PspID = configuration.PspID,
                   

            };
            var response = await apiClient.Post<VersioningRestResponse>(configuration.BaseUrl, "authentication", request
                  );
            return response;
        }


    }
}
