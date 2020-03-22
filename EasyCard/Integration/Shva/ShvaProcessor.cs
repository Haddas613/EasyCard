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

        public ShvaProcessor(IWebApiClient apiClient, IOptions<ShvaGlobalSettings> configuration, ILogger<ShvaProcessor> logger)
        {
            this.configuration = configuration.Value;

            this.apiClient = apiClient;

            this.logger = logger;
        }

        public async Task<ProcessorTransactionResponse> CreateTransaction(ProcessorTransactionRequest paymentTransactionRequest, string messageId, string
             correlationId, Func<IntegrationMessage, IntegrationMessage> handleIntegrationMessage = null)
        {
            ShvaTerminalSettings shvaParameters = paymentTransactionRequest.ProcessorSettings as ShvaTerminalSettings;
           

            var ashStartReq = shvaParameters.GetAshStartRequestBody();

            clsInput cls = paymentTransactionRequest.GetInitInitObjRequest();

            ashStartReq.inputObj = cls;
            ashStartReq.pinpad = new clsPinPad();
            ashStartReq.globalObj = new ShvaEMV.clsGlobal();

            var ashStartReqResult = await this.DoRequest(ashStartReq, AshStartUrl, messageId, correlationId, handleIntegrationMessage);

            var ashStartResultBody = (AshStartResponseBody)ashStartReqResult?.Body?.Content;

            if (ashStartResultBody == null)
            {
                // return failed response
                return new ProcessorTransactionResponse(Messages.EmptyResponse, RejectionReasonEnum.Unknown);
            }

            // Success situation
            if (ashStartResultBody.AshStartResult == (int)AshStartResultEnum.Success)
            {
                var ashAuthReq = ashStartResultBody.GetAshAuthRequestBody(shvaParameters);
                var ashEndReq = shvaParameters.GetAshEndRequestBody();

                ashStartReq.inputObj = ashEndReq.inputObj = cls;

                //call to the next function for make shva tansaction
                var resultAuth = await this.DoRequest(ashAuthReq, AshAuthUrl, messageId, correlationId, handleIntegrationMessage);
                var authResultBody = (AshAuthResponse)resultAuth?.Body?.Content;

                ashEndReq.globalObj = authResultBody.Body.globalObj;
                ashEndReq.pinpad = authResultBody.Body.pinpad;

                var resultAshEnd = await this.DoRequest(ashEndReq, AshEndUrl, messageId, correlationId, handleIntegrationMessage);
                var resultAshEndBody = (AshEndResponseBody)resultAshEnd?.Body?.Content;

                //   TODO: what should be done in this place ?
                //if (resultAshEndBody.AshEndResult == 777)
                //{
                //    if (resultStatus == 777 && transactionType == "11" && !billingModel.IsNewInitDeal && initDealResultModel != null)
                //    {
                //        מימוש עסקת אתחול שהצליחה, 0 כדי שתישמר בtblcard כרגיל לשידור
                //        resultStatus = 0;
                //    }
                //}

                return resultAshEndBody.GetProcessorTransactionResponse();
            }
            else if (ashStartResultBody.globalObj != null && ashStartResultBody.globalObj.outputObj != null && ashStartResultBody.globalObj.outputObj.ashStatus != null && ashStartResultBody.globalObj.outputObj.ashStatusDes != null)
            {
                return new ProcessorTransactionResponse(ashStartResultBody.globalObj.outputObj.ashStatus.valueTag, ashStartResultBody.globalObj.outputObj.ashStatusDes.valueTag);
            }
            else
            {
                return new ProcessorTransactionResponse("Unknown", RejectionReasonEnum.Unknown);
            }
        }

        /// <summary>
        /// Update Parameters in SHVA
        /// </summary>
        /// <param name="updateParamRequest"></param>
        /// <param name="messageId"></param>
        /// <param name="correlationId"></param>
        /// <param name="handleIntegrationMessage"></param>
        /// <returns></returns>
        public async Task ParamsUpdateTransaction(ShvaTerminalSettings updateParamRequest, string messageId, string
            correlationId, Func<IntegrationMessage, IntegrationMessage> handleIntegrationMessage = null)
        {
            var res = new ProcessorTransactionResponse();
            var updateParamsReq = new GetTerminalDataRequestBody();
            updateParamsReq.UserName = updateParamRequest.UserName;
            updateParamsReq.Password = updateParamRequest.Password;
            updateParamsReq.MerchantNumber = updateParamRequest.MerchantNumber;

            var result = await this.DoRequest(updateParamsReq, GetTerminalDataUrl, messageId, correlationId, handleIntegrationMessage);

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
        /// <param name="handleIntegrationMessage"></param>
        /// <returns></returns>
        public async Task<ShvaTransmissionResponse> TransmissionTransaction(ShvaTransmissionRequest transRequest, string messageId, string
            correlationId, Func<IntegrationMessage, IntegrationMessage> handleIntegrationMessage = null)
        {
            var res = new ShvaTransmissionResponse();
            ShvaTerminalSettings shvaParameters = transRequest.ProcessorSettings;
            var tranEMV = new TransEMVRequestBody();
            tranEMV.UserName = shvaParameters.UserName;
            tranEMV.Password = shvaParameters.Password;
            tranEMV.MerchantNumber = shvaParameters.MerchantNumber;
            tranEMV.DATA = transRequest.DATAToTrans;

            var result = await this.DoRequest(tranEMV, TransEMVUrl, messageId, correlationId, handleIntegrationMessage);

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

        protected async Task<Envelope> DoRequest(object request, string soapAction, string messageId, string correlationId, Func<IntegrationMessage, IntegrationMessage> handleIntegrationMessage = null)
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
                svcRes = await this.apiClient.PostXml<Envelope>(configuration.BaseUrl, $"/{soapAction}" /* TODO: please check which url Shva used for each request */, soap, () => BuildHeaders($"{configuration.BaseUrl}/{soapAction}"),
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
                    IntegrationMessage integrationMessage = new IntegrationMessage();

                    integrationMessage.MessageId = messageId;
                    integrationMessage.MessageDate = DateTime.UtcNow;
                    integrationMessage.Request = requestStr;
                    integrationMessage.Response = responseStr;
                    integrationMessage.ResponseStatus = responseStatusStr;
                    integrationMessage.Address = requestUrl;

                    handleIntegrationMessage?.Invoke(integrationMessage);
                }
            }

            return svcRes;
        }

        private async Task<NameValueCollection> BuildHeaders(string soapAction)
        {
            NameValueCollection headers = new NameValueCollection();

            // TODO: do we need any additional headers (?)
            //headers.Add("Authorization", new AuthenticationHeaderValue("Bearer", "TODO: AccessToken").ToString());

            headers.Add("SOAPAction", $"{soapAction}");

            return headers;
        }
    }
}
