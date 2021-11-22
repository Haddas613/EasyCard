using MerchantProfileApi.Models.Billing;
using MerchantProfileApi.Models.Terminal;
using Microsoft.Extensions.Options;
using Shared.Api.Configuration;
using Shared.Api.Models;
using Shared.Helpers;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MerchantProfileApi.Client
{
    public class MerchantMetadataApiClient
    {
        private readonly IWebApiClient webApiClient;
        private readonly ApiSettings apiConfiguration;
        //private readonly ILogger logger;
        private readonly IWebApiClientTokenService tokenService;

        public MerchantMetadataApiClient(IWebApiClient webApiClient, /*ILogger logger,*/ IWebApiClientTokenService tokenService, IOptions<ApiSettings> apiConfiguration)
        {
            this.webApiClient = webApiClient;
            //this.logger = logger;
            this.apiConfiguration = apiConfiguration.Value;
            this.tokenService = tokenService;
        }

        public NameValueCollection Headers { get; } = new NameValueCollection();

        // consumers

        public async Task<SummariesResponse<ConsumerSummary>> GetConsumers(ConsumersFilter filter)
        {
            var consumers = await webApiClient.Get<SummariesResponse<ConsumerSummary>>(apiConfiguration.MerchantProfileURL, $"/api/consumers", filter, BuildHeaders);

            return consumers;
        }

        public async Task<OperationResponse> CreateConsumer(ConsumerRequest request)
        {
            var consumerResp = await webApiClient.Post<OperationResponse>(apiConfiguration.MerchantProfileURL, $"/api/consumers", request, BuildHeaders);

            return consumerResp;
        }

        public async Task<OperationResponse> UpdateConsumer(UpdateConsumerRequest request)
        {
            var consumerResp = await webApiClient.Put<OperationResponse>(apiConfiguration.MerchantProfileURL, $"/api/consumers/{request.ConsumerID}", request, BuildHeaders);

            return consumerResp;
        }

        // items

        public async Task<SummariesResponse<ItemSummary>> GetItems(ItemsFilter filter)
        {
            var consumers = await webApiClient.Get<SummariesResponse<ItemSummary>>(apiConfiguration.MerchantProfileURL, $"/api/items", filter, BuildHeaders);

            return consumers;
        }

        public async Task<OperationResponse> CreateItem(ItemRequest request)
        {
            var consumerResp = await webApiClient.Post<OperationResponse>(apiConfiguration.MerchantProfileURL, $"/api/items", request, BuildHeaders);

            return consumerResp;
        }

        public async Task<OperationResponse> UpdateItem(UpdateItemRequest request)
        {
            var consumerResp = await webApiClient.Put<OperationResponse>(apiConfiguration.MerchantProfileURL, $"/api/items/{request.ItemID}", request, BuildHeaders);

            return consumerResp;
        }

        // terminal

        public async Task<SummariesResponse<TerminalSummary>> GetTerminals()
        {
            var terminals = await webApiClient.Get<SummariesResponse<TerminalSummary>>(apiConfiguration.MerchantProfileURL, $"/api/terminals", null, BuildHeaders);

            return terminals;
        }

        public async Task<TerminalResponse> GetTerminal(Guid terminalID)
        {
            var consumerResp = await webApiClient.Get<TerminalResponse>(apiConfiguration.MerchantProfileURL, $"/api/terminals/{terminalID}", null, BuildHeaders);

            return consumerResp;
        }

        public async Task<OperationResponse> UpdateTerminal(UpdateTerminalRequest request)
        {
            var consumerResp = await webApiClient.Put<OperationResponse>(apiConfiguration.MerchantProfileURL, $"/api/terminals/{request.TerminalID}", request, BuildHeaders);

            return consumerResp;
        }

        // common

        private async Task<NameValueCollection> BuildHeaders()
        {
            var token = await tokenService.GetToken();

            NameValueCollection headers = new NameValueCollection(Headers);

            if (token != null)
            {
                headers.Add("Authorization", new AuthenticationHeaderValue("Bearer", token.AccessToken).ToString());
            }

            return headers;
        }
    }
}
