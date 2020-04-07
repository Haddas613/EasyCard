using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api.Models;
using Shared.Helpers;
using Shared.Integration;
using Shared.Integration.Exceptions;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using Shva.Configuration;
using Shva.Conveters;
using Shva.Models;
using ShvaEMV;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Shva
{
    public class ShvaProcessor : IProcessor
    {
        private const string AshStartUrl = "AshStart";
        private const string AshAuthUrl = "AshAuth";
        private const string AshEndUrl = "AshEnd";
        private const string GetTerminalDataUrl = "GetTerminalData";
        private const string TransEMVUrl = "TransEMV";

        private readonly IWebApiClient apiClient;
        private readonly ShvaGlobalSettings configuration;
        private readonly ILogger logger;
        private readonly IIntegrationRequestLogStorageService integrationRequestLogStorageService;

        public ShvaProcessor(IWebApiClient apiClient, IOptions<ShvaGlobalSettings> configuration, ILogger<ShvaProcessor> logger, IIntegrationRequestLogStorageService integrationRequestLogStorageService)
        {
            this.configuration = configuration.Value;

            this.apiClient = apiClient;

            this.logger = logger;

            this.integrationRequestLogStorageService = integrationRequestLogStorageService;
        }

        public async Task<ProcessorCreateTransactionResponse> CreateTransaction(ProcessorCreateTransactionRequest paymentTransactionRequest, string messageId, string
             correlationId)
        {
            ShvaTerminalSettings shvaParameters = paymentTransactionRequest.ProcessorSettings.ToObject<ShvaTerminalSettings>();

            var ashStartReq = shvaParameters.GetAshStartRequestBody();

            clsInput cls = paymentTransactionRequest.GetInitInitObjRequest();

            ashStartReq.inputObj = cls;
            ashStartReq.pinpad = new clsPinPad();
            ashStartReq.globalObj = new ShvaEMV.clsGlobal(); // { shareD = new clsShareDetails { dealType = DealType.MAGNET } };

            var ashStartReqResult = await this.DoRequest(ashStartReq, AshStartUrl, messageId, correlationId, HandleIntegrationMessage);

            var ashStartResultBody = ashStartReqResult?.Body?.Content as AshStartResponseBody;

            if (ashStartResultBody == null)
            {
                // return failed response
                return new ProcessorCreateTransactionResponse(Messages.EmptyResponse, RejectionReasonEnum.Unknown, string.Empty);
            }

            // Success situation
            if (((AshStartResultEnum)ashStartResultBody.AshStartResult).IsSuccessful())
            {
            }
            else if (ashStartResultBody.globalObj?.outputObj?.ashStatus != null && ashStartResultBody.globalObj?.outputObj?.ashStatusDes != null)
            {
                return new ProcessorCreateTransactionResponse(ashStartResultBody.globalObj.outputObj.ashStatusDes.valueTag, ashStartResultBody.globalObj.outputObj.ashStatus.valueTag);
            }
            else
            {
                return new ProcessorCreateTransactionResponse(Messages.ResponseCannotBeParsed, RejectionReasonEnum.Unknown, ashStartResultBody.AshStartResult.ToString());
            }

            // auth request
            var ashAuthReq = ashStartResultBody.GetAshAuthRequestBody(shvaParameters);
            ashAuthReq.inputObj = cls;

            var resultAuth = await this.DoRequest(ashAuthReq, AshAuthUrl, messageId, correlationId, HandleIntegrationMessage);
            var authResultBody = resultAuth?.Body?.Content as AshAuthResponseBody;

            if (((AshAuthResultEnum)authResultBody.AshAuthResult).IsSuccessful())
            {
            }
            else if (authResultBody.globalObj?.outputObj?.ashStatus != null && authResultBody.globalObj?.outputObj?.ashStatusDes != null)
            {
                return new ProcessorCreateTransactionResponse(authResultBody.globalObj.outputObj.ashStatusDes.valueTag, authResultBody.globalObj.outputObj.ashStatus.valueTag);
            }
            else
            {
                return new ProcessorCreateTransactionResponse(Messages.ResponseCannotBeParsed, RejectionReasonEnum.Unknown, authResultBody.AshAuthResult.ToString());
            }

            // end request
            var ashEndReq = shvaParameters.GetAshEndRequestBody();

            ashEndReq.inputObj = cls;

            ashEndReq.globalObj = authResultBody.globalObj;
            ashEndReq.pinpad = authResultBody.pinpad;

            var resultAshEnd = await this.DoRequest(ashEndReq, AshEndUrl, messageId, correlationId, HandleIntegrationMessage);
            var resultAshEndBody = resultAshEnd?.Body?.Content as AshEndResponseBody;

            if (((AshEndResultEnum)resultAshEndBody.AshEndResult).IsSuccessful())
            {
                return resultAshEndBody.GetProcessorTransactionResponse();
            }
            else if (resultAshEndBody.globalObj?.outputObj?.ashStatus != null && resultAshEndBody.globalObj?.outputObj?.ashStatusDes != null)
            {
                return new ProcessorCreateTransactionResponse(resultAshEndBody.globalObj.outputObj.ashStatusDes.valueTag, resultAshEndBody.globalObj.outputObj.ashStatus.valueTag);
            }
            else
            {
                return new ProcessorCreateTransactionResponse(Messages.ResponseCannotBeParsed, RejectionReasonEnum.Unknown, resultAshEndBody.AshEndResult.ToString());
            }
        }

        /// <summary>
        /// Update Parameters in SHVA
        /// </summary>
        /// <param name="updateParamRequest"></param>
        /// <param name="messageId"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public async Task ParamsUpdateTransaction(ShvaTerminalSettings updateParamRequest, string messageId, string
            correlationId)
        {
            var res = new ProcessorCreateTransactionResponse();
            var updateParamsReq = new GetTerminalDataRequestBody();
            updateParamsReq.UserName = updateParamRequest.UserName;
            updateParamsReq.Password = updateParamRequest.Password;
            updateParamsReq.MerchantNumber = updateParamRequest.MerchantNumber;

            var result = await this.DoRequest(updateParamsReq, GetTerminalDataUrl, messageId, correlationId, HandleIntegrationMessage);

            var getTerminalDataResultBody = (GetTerminalDataResponseBody)result?.Body?.Content;

            if (getTerminalDataResultBody == null)
            {
                // TODO: error response
            }

            var code = getTerminalDataResultBody.GetTerminalDataResult;

            // TODO: validate response and return error is required response
        }

        /// <summary>
        /// TODO: Transmission method, It will be executed per merchant?
        /// </summary>
        /// <param name="transRequest"></param>
        /// <param name="messageId"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public async Task<ShvaTransmissionResponse> TransmissionTransaction(ShvaTransmissionRequest transRequest, string messageId, string
            correlationId)
        {
            var res = new ShvaTransmissionResponse();
            ShvaTerminalSettings shvaParameters = transRequest.ProcessorSettings;
            var tranEMV = new TransEMVRequestBody();
            tranEMV.UserName = shvaParameters.UserName;
            tranEMV.Password = shvaParameters.Password;
            tranEMV.MerchantNumber = shvaParameters.MerchantNumber;
            tranEMV.DATA = transRequest.DATAToTrans;

            var result = await this.DoRequest(tranEMV, TransEMVUrl, messageId, correlationId, HandleIntegrationMessage);

            var transResultBody = (TransEMVResponseBody)result?.Body?.Content;

            if (transResultBody == null)
            {
                return null;
            }

            res.ProcessorCode = transResultBody.TransEMVResult;
            res.BadTrans = transResultBody.BadTrans;
            res.RefNumber = transResultBody.RefNumber;
            res.Report = transResultBody.Report;
            res.TotalCreditTransSum = transResultBody.TotalCreditTransSum;
            res.TotalDebitTransSum = transResultBody.TotalDebitTransSum;
            res.TotalXML = transResultBody.TotalXML;
            return res;
        }

        protected async Task<Envelope> DoRequest(object request, string soapAction, string messageId, string correlationId, Func<IntegrationMessage, Task> handleIntegrationMessage = null)
        {
            var soap = new Envelope
            {
                Body = new Body
                {
                    Content = request
                },
            };

            Envelope svcRes = null;

            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;

            try
            {
                svcRes = await this.apiClient.PostXml<Envelope>(configuration.BaseUrl, string.Empty /* TODO: please check which url Shva used for each request */, soap, () => BuildHeaders($"{configuration.BaseUrl}/{soapAction}"),
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
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Exception {correlationId}: {ex.Message} ({messageId})");

                throw new IntegrationException("Shva integration request failed", messageId);
            }
            finally
            {
                if (handleIntegrationMessage != null)
                {
                    IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, Guid.NewGuid().GetSortableStr(DateTime.UtcNow), correlationId);

                    integrationMessage.Request = requestStr;
                    integrationMessage.Response = responseStr;
                    integrationMessage.ResponseStatus = responseStatusStr;
                    integrationMessage.Address = requestUrl;
                    integrationMessage.Action = soapAction;

                    await handleIntegrationMessage?.Invoke(integrationMessage);
                }
            }

            return svcRes;
        }

        private async Task<NameValueCollection> BuildHeaders(string soapAction)
        {
            NameValueCollection headers = new NameValueCollection();

            // TODO: do we need any additional headers (?)
            //headers.Add("Authorization", new AuthenticationHeaderValue("Bearer", "TODO: AccessToken").ToString());

            //headers.Add("SOAPAction", $"{soapAction}");

            return await Task.FromResult(headers);
        }

        private async Task HandleIntegrationMessage(IntegrationMessage msg)
        {
            await integrationRequestLogStorageService.Save(msg);
        }
    }
}
