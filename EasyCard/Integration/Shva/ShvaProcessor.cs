using Microsoft.Extensions.Logging;
using Shared.Api.Models;
using Shared.Helpers;
using Shared.Integration;
using Shared.Integration.Exceptions;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using Shva.Configuration;
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
        private const string BaseUrl = "http://shva.co.il/xmlwebservices/";
        private const string AshStartUrl = "AshStart";
        private const string AshAuthUrl = "AshAuth";
        private const string AshEndUrl = "AshEnd";
        private const string GetTerminalDataUrl = "GetTerminalData";//ShvaParamsUpdate
        private const string TransEMVUrl = "TransEMV";

        private readonly IWebApiClient apiClient;
        private readonly ShvaSettings configuration;
        private readonly ILogger logger;

        public ShvaProcessor(IWebApiClient apiClient, ShvaSettings configuration, ILogger<ShvaProcessor> logger)
        {
            this.configuration = configuration;

            this.apiClient = apiClient;

            this.logger = logger;
        }

        public async Task<ProcessorTransactionResponse> CreateTransaction(ProcessorTransactionRequest paymentTransactionRequest, string messageId, string
             correlationId, Func<IntegrationMessage, IntegrationMessage> handleIntegrationMessage = null)
        {
            var res = new ProcessorTransactionResponse();
            var ashStartReq = new AshStartRequestBody();
            ShvaParameters shvaParameters = (ShvaParameters)paymentTransactionRequest.ProcessorSettings;
            clsInput cls;
            InitInputObjRequest initObjReq;
            InitInitObjRequest(paymentTransactionRequest, ashStartReq, shvaParameters, out cls, out initObjReq);

            EMVDealHelper.InitInputObj(initObjReq, ref cls);
            ashStartReq.inputObj = cls;
            ashStartReq.pinpad = new clsPinPad();
            ashStartReq.globalObj = new ShvaEMV.clsGlobal();

            var result = await this.DoRequest(ashStartReq, string.Format("{0}{1}", BaseUrl, AshStartUrl), messageId, correlationId, handleIntegrationMessage);

            var ashStartResultBody = (AshStartResponseBody)result?.Body?.Content;

            if (ashStartResultBody == null)
            {
                return null;
            }


            if (ashStartResultBody.AshStartResult == 777)
            {
                var ashAuthReq = new AshAuthRequestBody();
                var ashEndReq = new AshEndRequestBody();
                ashAuthReq.pinpad = ashStartResultBody.pinpad;
                ashAuthReq.globalObj = ashStartResultBody.globalObj;
                ashStartReq.inputObj = ashEndReq.inputObj = cls;
                ashAuthReq.MerchantNumber = ashEndReq.MerchantNumber = shvaParameters.MerchantNumber;
                ashAuthReq.UserName = ashEndReq.UserName = shvaParameters.UserName;
                ashAuthReq.Password = ashEndReq.Password = shvaParameters.Password;

                var resultAuth = await this.DoRequest(ashAuthReq, string.Format("{0}{1}", BaseUrl, AshAuthUrl), messageId, correlationId, handleIntegrationMessage);

                var authResultBody = (AshAuthResponse)result?.Body?.Content;

                ashEndReq.globalObj = authResultBody.Body.globalObj;
                ashEndReq.pinpad = authResultBody.Body.pinpad;
                var resultAshEnd = await this.DoRequest(ashEndReq, string.Format("{0}{1}", BaseUrl, AshEndUrl), messageId, correlationId, handleIntegrationMessage);
                var resultAshEndBody = (AshEndResponseBody)result?.Body?.Content;
                int resCode = resultAshEndBody.AshEndResult;
                //if (resultAshEndBody.AshEndResult == 777)//   TODO
                //{
                //    if (resultStatus == 777 && transactionType == "11" && !billingModel.IsNewInitDeal && initDealResultModel != null)
                //    {
                //        מימוש עסקת אתחול שהצליחה, 0 כדי שתישמר בtblcard כרגיל לשידור
                //        resultStatus = 0;
                //    }
                //}

                res.ProcessorCode = resCode;
                res.DealNumber = resultAshEndBody.globalObj.receiptObj.voucherNumber.valueTag;
                res.TransactionReference = resultAshEndBody.globalObj.outputObj.tranRecord.valueTag;
            }

            return res;
        }


        public async Task<ExternalPaymentTransactionResponse> ParamsUpdateTransaction(ShvaParameters updateParamRequest, string messageId, string
            correlationId, Func<IntegrationMessage, IntegrationMessage> handleIntegrationMessage = null)
        {
            var res = new ExternalPaymentTransactionResponse();
            var updateParamsReq = new GetTerminalDataRequestBody();
            updateParamsReq.UserName = updateParamRequest.UserName;
            updateParamsReq.Password = updateParamRequest.Password;
            updateParamsReq.MerchantNumber = updateParamRequest.MerchantNumber;

            var result = await this.DoRequest(updateParamsReq, string.Format("{0}{1}", BaseUrl, GetTerminalDataUrl), messageId, correlationId, handleIntegrationMessage);

            var getTerminalDataResultBody = (GetTerminalDataResponseBody)result?.Body?.Content;

            if (getTerminalDataResultBody == null)
            {
                return null;
            }

            res.ProcessorCode = getTerminalDataResultBody.GetTerminalDataResult;
            return res;
        }

        public async Task<ExternalPaymentTransmissionResponse> TransactTransaction(ExternalPaymentTransTrasactionRequest transRequest, string messageId, string
            correlationId, Func<IntegrationMessage, IntegrationMessage> handleIntegrationMessage = null)
        {
            var res = new ExternalPaymentTransmissionResponse();
            ShvaParameters shvaParameters = (ShvaParameters)transRequest.ProcessorSettings;
            var tranEMV = new TransEMVRequestBody();
            tranEMV.UserName = shvaParameters.UserName;
            tranEMV.Password = shvaParameters.Password;
            tranEMV.MerchantNumber = shvaParameters.MerchantNumber;
            tranEMV.DATA = transRequest.DATAToTrans;

            var result = await this.DoRequest(tranEMV, string.Format("{0}{1}", BaseUrl, TransEMVUrl), messageId, correlationId, handleIntegrationMessage);

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


        private static void InitInitObjRequest(ExternalPaymentTransactionRequest paymentTransactionRequest, AshStartRequestBody ashStartReq, ShvaParameters shvaParameters, out clsInput cls, out InitInputObjRequest initObjReq)
        {
            ashStartReq.UserName = shvaParameters.UserName;
            ashStartReq.Password = shvaParameters.Password;
            ashStartReq.MerchantNumber = shvaParameters.MerchantNumber;
            cls = new clsInput();
            InitDealResultModel initDealResultModel = shvaParameters.IsNewInitDeal ? null : shvaParameters.InitDealModel;
            initObjReq = new InitInputObjRequest();
            initObjReq.ExpDate_YYMM = paymentTransactionRequest.ExpDate_YYMM;
            initObjReq.TransactionType = paymentTransactionRequest.TransactionType;
            initObjReq.Currency = paymentTransactionRequest.Currency;
            initObjReq.Code = paymentTransactionRequest.Code;
            initObjReq.CardNum = string.IsNullOrWhiteSpace(paymentTransactionRequest.CreditCardNumber) ? paymentTransactionRequest.Urack2 : paymentTransactionRequest.CreditCardNumber;
            initObjReq.CreditTerms = paymentTransactionRequest.CreditTerms;
            initObjReq.Amount = paymentTransactionRequest.Amount.ToString();
            initObjReq.Cvv2 = paymentTransactionRequest.CVV;
            initObjReq.AuthNum = shvaParameters.AuthNum;
            initObjReq.Id = paymentTransactionRequest.IdentityNumber;
            initObjReq.ParamJ = paymentTransactionRequest.ParamJ;
            initObjReq.NumOfPayment = paymentTransactionRequest.NumOfInstallment;
            initObjReq.FirstAmount = paymentTransactionRequest.FirstAmount;
            initObjReq.NonFirstAmount = paymentTransactionRequest.NonFirstAmount;
            initObjReq.InitDealM = initDealResultModel;
            initObjReq.IsNewInitDeal = shvaParameters.IsNewInitDeal;
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
                svcRes = await this.apiClient.PostXml<Envelope>(this.configuration.BaseUrl, "/Service/Service.asmx", soap, () => BuildHeaders(soapAction),
                    (url, request) => { requestStr = request; requestUrl = url; },
                    (response, responseStatus, responseHeaders) => { responseStr = response; responseStatusStr = responseStatus.ToString(); });


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

            // TODO: implement headers
            headers.Add("Authorization", new AuthenticationHeaderValue("Bearer", "TODO: AccessToken").ToString());

            headers.Add("SOAPAction", $"{soapAction}");


            return headers;
        }
    }
}
