using ClearingHouse.Converters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Helpers;
using Shared.Helpers.Security;
using Shared.Integration;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ClearingHouse
{
    public class ClearingHouseAggregator : IAggregator
    {
        private const string CreateTransactionRequest = "api/transaction";
        private const string CommitTransactionRequest = "api/transaction/{0}";
        private const string CancelTransactionRequest = "api/transaction/{0}/reject";

        private readonly IWebApiClient webApiClient;
        private readonly ClearingHouseGlobalSettings configuration;
        private readonly ILogger logger;
        private readonly IWebApiClientTokenService tokenService;
        private readonly IIntegrationRequestLogStorageService integrationRequestLogStorageService;

        public ClearingHouseAggregator(IWebApiClient webApiClient, ILogger<ClearingHouseAggregator> logger, IOptions<ClearingHouseGlobalSettings> configuration, IWebApiClientTokenService tokenService, IIntegrationRequestLogStorageService integrationRequestLogStorageService)
        {
            this.webApiClient = webApiClient;
            this.logger = logger;
            this.configuration = configuration.Value;
            this.tokenService = tokenService;
            this.integrationRequestLogStorageService = integrationRequestLogStorageService;
        }

        public async Task<AggregatorCreateTransactionResponse> CreateTransaction(AggregatorCreateTransactionRequest transactionRequest)
        {
            try
            {
                var request = transactionRequest.GetCreateTransactionRequest(configuration);

                var result = await webApiClient.Post<Models.OperationResponse>(configuration.ApiBaseAddress, CreateTransactionRequest, request, BuildHeaders);

                return result.GetAggregatorCreateTransactionResponse();
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError(clientError.Message);
                var result = clientError.TryConvert(new Models.OperationResponse { Message = clientError.Message });
                return result.GetAggregatorCreateTransactionResponse();
            }
        }

        public async Task<AggregatorCommitTransactionResponse> CommitTransaction(AggregatorCommitTransactionRequest transactionRequest)
        {
            try
            {
                var request = transactionRequest.GetCommitTransactionRequest(configuration);

                var result = await webApiClient.Put<Models.OperationResponse>(configuration.ApiBaseAddress, string.Format(CommitTransactionRequest, transactionRequest.AggregatorTransactionID), request, BuildHeaders);

                return result.GetAggregatorCommitTransactionResponse();
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError(clientError.Message);
                var result = clientError.TryConvert(new Models.OperationResponse { Message = clientError.Message });
                return result.GetAggregatorCommitTransactionResponse();
            }
        }

        public async Task<AggregatorCancelTransactionResponse> CancelTransaction(AggregatorCancelTransactionRequest transactionRequest)
        {
            try
            {
                var request = transactionRequest.GetCancelTransactionRequest(configuration);

                var result = await webApiClient.Post<Models.OperationResponse>(configuration.ApiBaseAddress, string.Format(CancelTransactionRequest, transactionRequest.AggregatorTransactionID), request, BuildHeaders);

                return result.GetAggregatorCancelTransactionResponse();
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError(clientError.Message);
                var result = clientError.TryConvert(new Models.OperationResponse { Message = clientError.Message });
                return result.GetAggregatorCancelTransactionResponse();
            }
        }

        public bool ShouldBeProcessedByAggregator(TransactionTypeEnum transactionType, SpecialTransactionTypeEnum specialTransactionType, JDealTypeEnum jDealType)
        {
            return jDealType == JDealTypeEnum.J4 && (specialTransactionType == SpecialTransactionTypeEnum.RegularDeal || specialTransactionType == SpecialTransactionTypeEnum.Refund);
        }

        private async Task<NameValueCollection> BuildHeaders()
        {
            var token = await tokenService.GetToken();

            NameValueCollection headers = new NameValueCollection();

            if (token != null)
            {
                headers.Add("Authorization", new AuthenticationHeaderValue("Bearer", token.AccessToken).ToString());
            }

            return headers;
        }
    }
}