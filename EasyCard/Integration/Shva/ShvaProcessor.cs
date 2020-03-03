﻿using Microsoft.Extensions.Logging;
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
        private readonly IWebApiClient apiClient;
        private readonly ShvaSettings configuration;
        private readonly ILogger logger;

        public ShvaProcessor(IWebApiClient apiClient, ShvaSettings configuration, ILogger<ShvaProcessor> logger)
        {
            this.configuration = configuration;

            this.apiClient = apiClient;

            this.logger = logger;
        }

        public async Task<ExternalPaymentTransactionResponse> CreateTransaction(ExternalPaymentTransactionRequest paymentTransactionRequest, string messageId, string
             correlationId, Func<IntegrationMessage, IntegrationMessage> handleIntegrationMessage = null)
        {
            var res = new ExternalPaymentTransactionResponse();
            var ashStartReq = new AshStartRequestBody();
            ShvaParameters shvaParameters = (ShvaParameters)paymentTransactionRequest.ProcessorSettings;
            ashStartReq.UserName = shvaParameters.UserName;
            ashStartReq.Password = shvaParameters.Password;
            ashStartReq.MerchantNumber = shvaParameters.MerchantNumber;
            clsInput cls = new clsInput();
            InitDealResultModel initDealResultModel = new InitDealResultModel();//TODO   billingModel.IsNewInitDeal ? null : Common.BL.Billing.GetInitDealCardDataByCustomerID(billingModel.CustomerID);
            EMVDealHelper.InitInputObj(paymentTransactionRequest.ExpDate_YYMM, paymentTransactionRequest.TransactionType, paymentTransactionRequest.Currency, paymentTransactionRequest.Code,
                String.IsNullOrWhiteSpace(paymentTransactionRequest.CreditCardNumber) ? paymentTransactionRequest.Urack2 : paymentTransactionRequest.CreditCardNumber,
               paymentTransactionRequest.CreditTerms, paymentTransactionRequest.Amount.ToString(), paymentTransactionRequest.CVV, shvaParameters.AuthNum, paymentTransactionRequest.IdentityNumber, paymentTransactionRequest.ParamJ, paymentTransactionRequest.NumOfInstallment, paymentTransactionRequest.FirstAmount,
               paymentTransactionRequest.NonFirstAmount, initDealResultModel, shvaParameters.IsNewInitDeal, ref cls);
            ashStartReq.inputObj = cls;
            ashStartReq.pinpad = new clsPinPad();
            ashStartReq.globalObj = new ShvaEMV.clsGlobal();

            var result = await this.DoRequest(ashStartReq, "http://shva.co.il/xmlwebservices/AshStart", messageId, correlationId, handleIntegrationMessage);

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

                var resultAuth = await this.DoRequest(ashAuthReq, "http://shva.co.il/xmlwebservices/AshAuth", messageId, correlationId, handleIntegrationMessage);

                var authResultBody = (AshAuthResponse)result?.Body?.Content;

                ashEndReq.globalObj = authResultBody.Body.globalObj;
                ashEndReq.pinpad = authResultBody.Body.pinpad;
                var resultAshEnd = await this.DoRequest(ashEndReq, "http://shva.co.il/xmlwebservices/AshEnd", messageId, correlationId, handleIntegrationMessage);
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
               
                res.ShvaCode = resCode;
                res.DealNumber = resultAshEndBody.globalObj.receiptObj.voucherNumber.valueTag;
                res.TransactionReference = resultAshEndBody.globalObj.outputObj.tranRecord.valueTag;
            }

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
