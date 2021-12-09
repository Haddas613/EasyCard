using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api.Models;
using Shared.Helpers;
using Shared.Integration;
using Shared.Integration.Exceptions;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using Shva.Conveters;
using Shva.Models;
using ShvaEMV;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Shva
{
    public class ShvaProcessor : IProcessor
    {
        private const string AshStartUrl = "AshStart";
        private const string AshAuthUrl = "AshAuth";
        private const string AshEndUrl = "AshEnd";
        private const string GetTerminalDataUrl = "GetTerminalData";
        private const string ChangePasswordUrl = "ChangePassword";
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

        public async Task<ProcessorCreateTransactionResponse> CreateTransaction(ProcessorCreateTransactionRequest paymentTransactionRequest)
        {
            ShvaTerminalSettings shvaParameters = paymentTransactionRequest.ProcessorSettings as ShvaTerminalSettings;

            if (shvaParameters == null)
            {
                throw new ArgumentNullException("ShvaTerminalSettings (at paymentTransactionRequest.ProcessorSettings) is required");
            }

            var ashStartReq = shvaParameters.GetAshStartRequestBody();

            clsInput cls = paymentTransactionRequest.GetInitInitObjRequest();

            ashStartReq.inputObj = cls;
            ashStartReq.pinpad = new clsPinPad();
            ashStartReq.globalObj = new ShvaEMV.clsGlobal(); // { shareD = new clsShareDetails { dealType = DealType.MAGNET } };

            var ashStartReqResult = await this.DoRequest(ashStartReq, AshStartUrl, paymentTransactionRequest.PaymentTransactionID, paymentTransactionRequest.CorrelationId, HandleIntegrationMessage);

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
                return new ProcessorCreateTransactionResponse(ashStartResultBody.globalObj.outputObj.ashStatusDes.valueTag, ashStartResultBody.globalObj.outputObj.ashStatus.valueTag, ashStartResultBody.AshStartResult);
            }
            else
            {
                return new ProcessorCreateTransactionResponse(Messages.CannotGetErrorCodeFromResponse, RejectionReasonEnum.Unknown, ashStartResultBody.AshStartResult.ToString(), ashStartResultBody.AshStartResult);
            }

            if (((AshStartResultEnum)ashStartResultBody.AshStartResult).IsSuccessForContinue())
            {
                // auth request
                var ashAuthReq = ashStartResultBody.GetAshAuthRequestBody(shvaParameters);
                ashAuthReq.inputObj = cls;

                var resultAuth = await this.DoRequest(ashAuthReq, AshAuthUrl, paymentTransactionRequest.PaymentTransactionID, paymentTransactionRequest.CorrelationId, HandleIntegrationMessage);
                var authResultBody = resultAuth?.Body?.Content as AshAuthResponseBody;

                if (((AshAuthResultEnum)authResultBody.AshAuthResult).IsSuccessful())
                {
                }
                else if (authResultBody.AshAuthResult.IsOKNumberRequired(authResultBody.globalObj?.outputObj?.telAuthAbility?.valueTag))
                {
                    return authResultBody.GetOKNumberRequiredProcessorTransactionResponse();
                }
                else if (authResultBody.globalObj?.outputObj?.ashStatus != null && authResultBody.globalObj?.outputObj?.ashStatusDes != null)
                {
                    return new ProcessorCreateTransactionResponse(authResultBody.globalObj.outputObj.ashStatusDes.valueTag, authResultBody.globalObj.outputObj.ashStatus.valueTag, authResultBody.AshAuthResult);
                }
                else
                {
                    return new ProcessorCreateTransactionResponse(Messages.CannotGetErrorCodeFromResponse, RejectionReasonEnum.Unknown, authResultBody.AshAuthResult.ToString(), authResultBody.AshAuthResult);
                }

                // end request
                var ashEndReq = shvaParameters.GetAshEndRequestBody();

                ashEndReq.inputObj = cls;

                ashEndReq.globalObj = authResultBody.globalObj;
                ashEndReq.pinpad = authResultBody.pinpad;

                var resultAshEnd = await this.DoRequest(ashEndReq, AshEndUrl, paymentTransactionRequest.PaymentTransactionID, paymentTransactionRequest.CorrelationId, HandleIntegrationMessage);
                var resultAshEndBody = resultAshEnd?.Body?.Content as AshEndResponseBody;

                if (((AshEndResultEnum)resultAshEndBody.AshEndResult).IsSuccessful())
                {
                    return resultAshEndBody.GetProcessorTransactionResponse();
                }
                else if (resultAshEndBody.AshEndResult.IsOKNumberRequired(resultAshEndBody.globalObj?.outputObj?.telAuthAbility?.valueTag))
                {
                    return resultAshEndBody.GetOKNumberRequiredProcessorTransactionResponse();
                }
                else if (resultAshEndBody.globalObj?.outputObj?.ashStatus != null && resultAshEndBody.globalObj?.outputObj?.ashStatusDes != null)
                {
                    return new ProcessorCreateTransactionResponse(resultAshEndBody.globalObj.outputObj.ashStatusDes.valueTag, resultAshEndBody.globalObj.outputObj.ashStatus.valueTag, resultAshEndBody.AshEndResult);
                }
                else
                {
                    return new ProcessorCreateTransactionResponse(Messages.CannotGetErrorCodeFromResponse, RejectionReasonEnum.Unknown, resultAshEndBody.AshEndResult.ToString(), resultAshEndBody.AshEndResult);
                }
            }
            else /* (((AshStartResultEnum)ashStartResultBody.AshStartResult).IsSuccessful())*/
            {
                return ashStartResultBody.GetProcessorTransactionResponse();
            }
        }

        /// <summary>
        /// Update Parameters in SHVA
        /// </summary>
        /// <param name="updateParamRequest"></param>
        /// <returns></returns>
        public async Task<ProcessorUpdateParamteresResponse> ParamsUpdateTransaction(ProcessorUpdateParametersRequest updateParamRequest)
        {
            var res = new ProcessorCreateTransactionResponse();
            ShvaTerminalSettings shvaParameters = updateParamRequest.ProcessorSettings as ShvaTerminalSettings;
            var updateParamsReq = new GetTerminalDataRequestBody();
            updateParamsReq.UserName = shvaParameters.UserName;
            updateParamsReq.Password = shvaParameters.Password;
            updateParamsReq.MerchantNumber = shvaParameters.MerchantNumber;

            var result = await this.DoRequest(updateParamsReq, GetTerminalDataUrl, updateParamRequest.TerminalID.ToString(), updateParamRequest.CorrelationId, HandleIntegrationMessage);

            var getTerminalDataResultBody = (GetTerminalDataResponseBody)result?.Body?.Content;

            if (getTerminalDataResultBody == null)
            {
                return new ProcessorUpdateParamteresResponse() { Success = false };
            }

            return new ProcessorUpdateParamteresResponse()
            {
                Success = getTerminalDataResultBody.GetTerminalDataResult == 0,
                Code = getTerminalDataResultBody.GetTerminalDataResult
            };
        }

        public Task<ProcessorPreCreateTransactionResponse> PreCreateTransaction(ProcessorCreateTransactionRequest paymentTransactionRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<ProcessorTransmitTransactionsResponse> TransmitTransactions(ProcessorTransmitTransactionsRequest transmitTransactionsRequest)
        {
            ShvaTerminalSettings shvaParameters = transmitTransactionsRequest.ProcessorSettings as ShvaTerminalSettings;

            if (shvaParameters == null)
            {
                throw new ArgumentNullException("ShvaTerminalSettings (at transmitTransactionsRequest.ProcessorSettings) is required");
            }

            var res = new ShvaTransmissionResponse();

            var tranEMV = new TransEMVRequestBody();
            tranEMV.UserName = shvaParameters.UserName;
            tranEMV.Password = shvaParameters.Password;
            tranEMV.MerchantNumber = shvaParameters.MerchantNumber;
            tranEMV.DATA = string.Join(";", transmitTransactionsRequest.TransactionIDs);

            var result = await this.DoRequest(tranEMV, TransEMVUrl, transmitTransactionsRequest.TerminalID.ToString(), transmitTransactionsRequest.CorrelationId, HandleIntegrationMessage);

            var transResultBody = (TransEMVResponseBody)result?.Body?.Content;

            if (transResultBody == null)
            {
                throw new IntegrationException("Empty transmission response", null); // TODO: integration message ID
            }

            res.ProcessorCode = transResultBody.TransEMVResult;
            res.FailedTransactions = transResultBody.BadTrans?.Split(new string[] { ";", " " }, StringSplitOptions.RemoveEmptyEntries);
            res.TransmissionReference = transResultBody.RefNumber;
            res.Report = transResultBody.Report;
            res.TotalCreditTransSum = transResultBody.TotalCreditTransSum;
            res.TotalDebitTransSum = transResultBody.TotalDebitTransSum;
            res.TotalXML = transResultBody.TotalXML;

            // TODO: failed case (?)

            return res;
        }

        public async Task<ProcessorChangePasswordResponse> ChangePassword(ProcessorChangePasswordRequest changePasswordRequest)
        {
            ShvaTerminalSettings shvaParameters = changePasswordRequest.ProcessorSettings as ShvaTerminalSettings;

            if (shvaParameters == null)
            {
                throw new ArgumentNullException("ShvaTerminalSettings (at ProcessorChangePasswordRequest.ProcessorSettings) is required");
            }

            var changePasswordReq = shvaParameters.GetChangePasswordRequestBody(changePasswordRequest.NewPassword);

            var changePasswordReqResult = await this.DoRequest(changePasswordReq, ChangePasswordUrl, changePasswordRequest.TerminalID.ToString(), changePasswordRequest.CorrelationId, HandleIntegrationMessage);

            var changePasswordResultBody = changePasswordReqResult?.Body?.Content as ChangePasswordResponseBody;

            if (changePasswordResultBody == null)
            {
                // return failed response
                return new ProcessorChangePasswordResponse(Messages.EmptyResponse, RejectionReasonEnum.Unknown, string.Empty);
            }

            // Success situation
            if (((ChangePasswordResultEnum)changePasswordResultBody.ChangePasswordResult).IsSuccessful())
            {
                return changePasswordResultBody.GetProcessorChangePasswordResponse();
            }
            else
            {
                return new ProcessorChangePasswordResponse(Messages.CannotGetErrorCodeFromResponse, RejectionReasonEnum.Unknown, changePasswordResultBody.ChangePasswordResult.ToString());
            }
        }

        public Task<IEnumerable<IntegrationMessage>> GetStorageLogs(string entityID)
        {
            return integrationRequestLogStorageService.GetAll(entityID);
        }

        protected async Task<Envelope> DoRequest(object request, string soapAction, string entityID, string correlationId, Func<IntegrationMessage, Task> handleIntegrationMessage = null)
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

            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            var actionPath = configuration.BaseUrl.EndsWith(".asmx") ? string.Empty : $"{soapAction}";

            try
            {
                svcRes = await this.apiClient.PostXml<Envelope>(configuration.BaseUrl, actionPath, soap, () => BuildHeaders(soapAction),
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
                this.logger.LogError(ex, $"Shva integration request failed ({integrationMessageId}): {ex.Message}");

                throw new IntegrationException("Shva integration request failed", integrationMessageId);
            }
            finally
            {
                if (handleIntegrationMessage != null)
                {
                    IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, entityID, integrationMessageId, correlationId);

                    //Do not expose credit card and cvv numbers in log
                    requestStr = Regex.Replace(requestStr, "\\<clientInputPan\\>\\d{9,16}\\</clientInputPan\\>", "<clientInputPan>****************</clientInputPan>");
                    requestStr = Regex.Replace(requestStr, "\\<cvv2\\>\\d{3,4}\\</cvv2\\>", "<cvv2>***</cvv2>");

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

            headers.Add("SOAPAction", $"http://shva.co.il/xmlwebservices/{soapAction}");

            return await Task.FromResult(headers);
        }

        private async Task HandleIntegrationMessage(IntegrationMessage msg)
        {
            await integrationRequestLogStorageService.Save(msg);
        }
    }
}
