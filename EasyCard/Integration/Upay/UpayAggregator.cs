using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nancy.Json;
using Newtonsoft.Json;
using Shared.Helpers;
using Shared.Helpers.Security;
using Shared.Integration;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Upay;
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
        private readonly IMapper mapper;

        public UpayAggregator(IWebApiClient webApiClient, ILogger<UpayAggregator> logger, IOptions<UpayGlobalSettings> configuration, IWebApiClientTokenService tokenService, IIntegrationRequestLogStorageService integrationRequestLogStorageService, IMapper mapper)
        {
            this.webApiClient = webApiClient;
            this.logger = logger;
            this.configuration = configuration.Value;
            this.tokenService = tokenService;
            this.integrationRequestLogStorageService = integrationRequestLogStorageService;
            this.mapper = mapper;
        }
        public async Task<AggregatorCreateTransactionResponse> CreateTransaction(AggregatorCreateTransactionRequest transactionRequest)
        {
            try
            {
                var upaySettings = transactionRequest.AggregatorSettings as UpayTerminalSettings;

                if (transactionRequest.TransactionType == TransactionTypeEnum.Credit || transactionRequest.TransactionType == TransactionTypeEnum.Immediate)
                {
                    return new AggregatorCreateTransactionResponse
                    {
                        Success = false,
                        ErrorMessage = "Transaction type is not valid for upay transactions"
                    };
                }

                var CreateTransactionRequest = UpayHelper.GetCreateTranMsgModel(upaySettings.Email, UpayHelper.GetStringInMD5(upaySettings.Password), !configuration.LiveSystem, upaySettings.AuthenticateKey, transactionRequest);
                var loginRequest = UpayHelper.GetLoginMsgModel(upaySettings.Email, UpayHelper.GetStringInMD5(upaySettings.Password), !configuration.LiveSystem, upaySettings.AuthenticateKey);
                var Msgs = new MsgModel[2];
                Msgs[0] = loginRequest;
                Msgs[1] = CreateTransactionRequest;
                JavaScriptSerializer serDes = new JavaScriptSerializer();
                var jsonResult = serDes.Serialize(Msgs);
                IDictionary<string, string> credentials = new Dictionary<string, string>();
                credentials.Add("msgs", jsonResult);

                string requestUrl = null;
                string requestStr = null;
                string responseStr = null;
                string responseStatusStr = null;
                var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

                string result = null;
                try
                {
                    result = await webApiClient.PostRawForm(configuration.ApiBaseAddress, "", credentials, null,
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
                finally
                {
                    IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, integrationMessageId, transactionRequest.CorrelationId);

                    integrationMessage.Request = jsonResult;
                    integrationMessage.Response = responseStr;
                    integrationMessage.ResponseStatus = responseStatusStr;
                    integrationMessage.Address = requestUrl;

                    await integrationRequestLogStorageService.Save(integrationMessage);
                }

                TranResponseFullModel resultUpay = JsonConvert.DeserializeObject<TranResponseFullModel>(result);
                return resultUpay.GetAggregatorCreateTransactionResponse();
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
        }

        /*
        public async Task<Object> Login(UpayGlobalSettings configuration, UpayTerminalSettings settings)
        {
            var loginRequest = UpayHelper.GetLoginMsgModel(settings.Email, UpayHelper.GetStringInMD5(settings.Password), true, settings.AuthenticateKey);
            JavaScriptSerializer serDes = new JavaScriptSerializer();
            var jsonResult = serDes.Serialize(loginRequest);
            var kj = String.Format("msg={0}", jsonResult);

            var credentials = new FormUrlEncodedContent(new[]
            {new KeyValuePair<string, string>("msg",jsonResult) });
            var result = await webApiClient.PostFormData<Models.OperationResponse>(configuration.ApiBaseAddress, "", loginRequest,null,null,null, credentials);
            return null;// result;
        }
    */


        public async Task<AggregatorCommitTransactionResponse> CommitTransaction(AggregatorCommitTransactionRequest transactionRequest)
        {
            try
            {
                var shvaDetails = mapper.Map<Upay.Models.PaymentGatewayAdditionalDetails>(transactionRequest.ProcessorTransactionDetails);

                var upaySettings = transactionRequest.AggregatorSettings as UpayTerminalSettings;

                var CommitTransactionRequest = UpayHelper.GetCommitTranMsgModel(upaySettings.Email, UpayHelper.GetStringInMD5(upaySettings.Password), !configuration.LiveSystem, upaySettings.AuthenticateKey, transactionRequest, shvaDetails);
                var loginRequest = UpayHelper.GetLoginMsgModel(upaySettings.Email, UpayHelper.GetStringInMD5(upaySettings.Password), !configuration.LiveSystem, upaySettings.AuthenticateKey);
                var Msgs = new MsgModel[2];
                Msgs[0] = loginRequest;
                Msgs[1] = CommitTransactionRequest;
                JavaScriptSerializer serDes = new JavaScriptSerializer();
                var SerializerSettings = new JsonSerializerSettings();
                SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                var jsonResult = serDes.Serialize(Msgs);
                IDictionary<string, string> credentials = new Dictionary<string, string>();
                credentials.Add("msgs", jsonResult);

                string requestUrl = null;
                string requestStr = null;
                string responseStr = null;
                string responseStatusStr = null;
                var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

                string result = null;
                try
                {
                    result = await webApiClient.PostRawForm(configuration.ApiBaseAddress, "", credentials, null,
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
                finally
                {
                    IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, integrationMessageId, transactionRequest.CorrelationId);

                    integrationMessage.Request = jsonResult;
                    integrationMessage.Response = responseStr;
                    integrationMessage.ResponseStatus = responseStatusStr;
                    integrationMessage.Address = requestUrl;

                    await integrationRequestLogStorageService.Save(integrationMessage);
                }

                TranResponseFullModel resultUpay = JsonConvert.DeserializeObject<TranResponseFullModel>(result);
                return resultUpay.GetAggregatorCommitTransactionResponse();
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError(clientError.Message);
                var result = clientError.TryConvert(new Models.OperationResponse { ErrorMessage = clientError.Message, ErrorDescription = clientError.Message });
                return result.GetAggregatorCommitTransactionResponse();
            }
        }

        public async Task<AggregatorCancelTransactionResponse> CancelTransaction(AggregatorCancelTransactionRequest transactionRequest)
        {
            return await Task.FromResult( new AggregatorCancelTransactionResponse { Success = true }); //no cancel transaction with upay
        }

        public bool ShouldBeProcessedByAggregator(Shared.Integration.Models.TransactionTypeEnum transactionType, SpecialTransactionTypeEnum specialTransactionType, JDealTypeEnum jDealType)
        {
            return jDealType == JDealTypeEnum.J4 && (specialTransactionType == SpecialTransactionTypeEnum.RegularDeal || specialTransactionType == SpecialTransactionTypeEnum.Refund || specialTransactionType == SpecialTransactionTypeEnum.InitialDeal);
        }

        public Task<AggregatorTransactionResponse> GetTransaction(string aggregatorTransactionID)
        {
            throw new NotImplementedException();
        }
    }
}
