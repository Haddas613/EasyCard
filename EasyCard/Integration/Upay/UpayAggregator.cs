using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Helpers;
using Shared.Helpers.Security;
using Shared.Integration;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using System;
using System.Threading.Tasks;
using Upay;
using Upay.Converters;
using Upay.Models;

namespace Upay
{
    public class UpayAggregator : IAggregator
    {
        private const string CreateTransactionRequest = "REDIRECTDEPOSITCREDITCARDTRANSFER";
        private const string LoginTransactionRequest = "LOGIN";
        private const string GetCommissionsTableRequest = "GETDEPOSITCREDITCARDTRANSFERCLEARINGTABLES";

        private const string CalculateCommissionRequest = "CALCULATECOMMISSION";
        private const string GetKeyRequest = "GETKEY";

        private readonly IWebApiClient webApiClient;
        private readonly UpayGlobalSettings configuration;
        private readonly ILogger logger;
        private readonly IWebApiClientTokenService tokenService;
        private readonly IIntegrationRequestLogStorageService integrationRequestLogStorageService;
        public UpayAggregator(IWebApiClient webApiClient, ILogger<UpayAggregator> logger, IOptions<UpayGlobalSettings> configuration, IWebApiClientTokenService tokenService, IIntegrationRequestLogStorageService integrationRequestLogStorageService)
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
                var upaySettings = transactionRequest.AggregatorSettings as UpayTerminalSettings;

                var loginRequest = UpayHelper.GetLoginMsgModel(upaySettings.Email, UpayHelper.GetStringInMD5(upaySettings.Password), true/*TODO*/, upaySettings.AuthenticateKey);

                var result = await webApiClient.Post<Models.OperationResponse>(configuration.ApiBaseAddress, "", loginRequest);
                return result.GetAggregatorCreateTransactionResponse();
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError($"{clientError.Message}: {clientError.Response}");

                OperationResponse result;

                if (clientError.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    result = clientError.TryConvert(new Models.OperationResponse
                    {
                        ErrorMessage = clientError.Message
                    });
                }
                else
                {
                    result = new OperationResponse { ErrorMessage = clientError.Response };
                }

                return result.GetAggregatorCreateTransactionResponse();
            }
            /*
            try
            {
                var request = transactionRequest.GetCreateTransactionRequest(configuration);

                var result = await webApiClient.Post<Models.OperationResponse>(configuration.ApiBaseAddress, CreateTransactionRequest, request);

                return result.GetAggregatorCreateTransactionResponse();
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError($"{clientError.Message}: {clientError.Response}");

                OperationResponse result;

                if (clientError.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    result = clientError.TryConvert(new Models.OperationResponse { Message = clientError.Message });
                }
                else
                {
                    result = new OperationResponse { Message = clientError.Response };
                }

                return result.GetAggregatorCreateTransactionResponse();
            }
            */

        }

        public async Task<Object> Login(UpayGlobalSettings configuration, UpayTerminalSettings settings)
        {
            var loginRequest = UpayHelper.GetLoginMsgModel(settings.Email, UpayHelper.GetStringInMD5(settings.Password), true/*TODO*/, settings.AuthenticateKey);

            var result = await webApiClient.Post<Models.OperationResponse>(configuration.ApiBaseAddress, "", loginRequest);
            return result;
        }


        public async Task<AggregatorCommitTransactionResponse> CommitTransaction(AggregatorCommitTransactionRequest transactionRequest)
        {
            return null;//TODO 
        }

        public async Task<AggregatorCancelTransactionResponse> CancelTransaction(AggregatorCancelTransactionRequest transactionRequest)
        {
            return null;//TODO
        }

        public bool ShouldBeProcessedByAggregator(Shared.Integration.Models.TransactionTypeEnum transactionType, SpecialTransactionTypeEnum specialTransactionType, JDealTypeEnum jDealType)
        {
            return jDealType == JDealTypeEnum.J4 && (specialTransactionType == SpecialTransactionTypeEnum.RegularDeal || specialTransactionType == SpecialTransactionTypeEnum.Refund || specialTransactionType == SpecialTransactionTypeEnum.InitialDeal);
        }

        public async Task<AggregatorTransactionResponse> GetTransaction(string aggregatorTransactionID)
        {
            return null;//TODO!!
        }
    }
}
