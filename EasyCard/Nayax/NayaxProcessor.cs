using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nayax.Configuration;
using Nayax.Converters;
using Nayax.Models;
using Shared.Helpers;
using Shared.Integration;
using Shared.Integration.Exceptions;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Transactions.Business.Services;

namespace Nayax
{
    public class NayaxProcessor : IProcessor
    {
        private const string Phase1Url = "doTransactionPhase1";
        private const string Phase2Url = "doTransactionPhase2";

        private readonly IWebApiClient apiClient;
        private readonly NayaxGlobalSettings configuration;
        private readonly ILogger logger;
        private readonly IIntegrationRequestLogStorageService integrationRequestLogStorageService;

        public NayaxProcessor(IWebApiClient apiClient, IOptions<NayaxGlobalSettings> configuration, ILogger<NayaxProcessor> logger, IIntegrationRequestLogStorageService integrationRequestLogStorageService)
        {
            this.configuration = configuration.Value;

            this.apiClient = apiClient;// new webapiclient(); TODO

            this.logger = logger;

            this.integrationRequestLogStorageService = integrationRequestLogStorageService;
        }

        /// <summary>
        /// especially for pinpad
        /// </summary>
        /// <param name="paymentTransactionRequest"></param>
        /// <returns></returns>
        public async Task<ProcessorPreCreateTransactionResponse> PreCreateTransaction(ProcessorCreateTransactionRequest paymentTransactionRequest, string sysTraceNum)
        {
            NayaxTerminalSettings nayaxParameters = paymentTransactionRequest.PinPadProcessorSettings as NayaxTerminalSettings;

            if (nayaxParameters == null)
            {
                throw new ArgumentNullException("NayaxTerminalSettings (at paymentTransactionRequest.ProcessorSettings) is required");
            }

            var phas1Req = nayaxParameters.GetPhase1RequestBody(configuration,paymentTransactionRequest.EasyCardTerminalID);
             ObjectInPhase1RequestParams params2 = paymentTransactionRequest.GetObjectInPhase1RequestParams(sysTraceNum);
           
            phas1Req.paramss[1] = params2;
            //client.Timeout = TimeSpan.FromSeconds(30); TODO timeout for 30 minutes
            var phase1ReqResult = await this.apiClient.Post<Models.Phase1ResponseBody>(configuration.BaseUrl, Phase1Url, phas1Req, BuildHeaders);//this.DoRequest(phas1Req, Phase1Url, paymentTransactionRequest.CorrelationId, HandleIntegrationMessage);

            var phase1ResultBody = phase1ReqResult as Phase1ResponseBody;

            if (phase1ResultBody == null)
            {
                // return failed response
                return new ProcessorPreCreateTransactionResponse(" ", RejectionReasonEnum.Unknown, string.Empty);
            }
            int statusPreCreate;
            if (!Int32.TryParse(phase1ResultBody.statusCode, out statusPreCreate))
            {
                // return failed response
                return new ProcessorPreCreateTransactionResponse("Status code is not valid", RejectionReasonEnum.Unknown, string.Empty);
            }
            // end request


            if (((PhaseResultEnum)Convert.ToInt32(phase1ResultBody.statusCode)).IsSuccessful())
            {
                return phase1ResultBody.GetProcessorPreTransactionResponse();
            }
            else if (!String.IsNullOrEmpty(phase1ResultBody?.statusCode) && !String.IsNullOrEmpty(phase1ResultBody?.statusMessage))
            {
                return new ProcessorPreCreateTransactionResponse(phase1ResultBody?.statusMessage, phase1ResultBody?.statusCode);
            }
            else
            {
                return new ProcessorPreCreateTransactionResponse(Messages.CannotGetErrorCodeFromResponse, RejectionReasonEnum.Unknown, phase1ResultBody.statusCode);
            }

        }

        public async Task<ProcessorCreateTransactionResponse> CreateTransaction(ProcessorCreateTransactionRequest paymentTransactionRequest)
        {
            NayaxTerminalSettings nayaxParameters = paymentTransactionRequest.PinPadProcessorSettings as NayaxTerminalSettings;

            if (nayaxParameters == null)
            {
                throw new ArgumentNullException("NayaxTerminalSettings (at paymentTransactionRequest.ProcessorSettings) is required");
            }

            var phase2Req = nayaxParameters.GetPhase2RequestBody(configuration);

            ObjectInPhase2RequestParams params2 = paymentTransactionRequest.GetObjectInPhase2RequestParams();

            phase2Req.paramss[1] = params2;


            var phase2ReqResult = await this.apiClient.Post<Models.Phase2ResponseBody>(configuration.BaseUrl, Phase2Url, phase2Req);//this.DoRequest(phas1Req, Phase1Url, paymentTransactionRequest.CorrelationId, HandleIntegrationMessage);

            var phase2ResultBody = phase2ReqResult as Phase2ResponseBody;

            if (phase2ResultBody == null)
            {
                // return failed response
                return new ProcessorCreateTransactionResponse(Messages.EmptyResponse, RejectionReasonEnum.Unknown, string.Empty);
            }
            int statusCreate;
            if (!Int32.TryParse(phase2ResultBody.statusCode, out statusCreate))
            {
                // return failed response
                return new ProcessorCreateTransactionResponse(Messages.StatusCodeIsNotValid, RejectionReasonEnum.Unknown, phase2ResultBody.statusCode);
            }

            if (((PhaseResultEnum)Convert.ToInt32(phase2ResultBody.statusCode)).IsSuccessful())
            {
                return phase2ResultBody.GetProcessorTransactionResponse();
            }
            else if (!String.IsNullOrEmpty(phase2ResultBody?.statusCode) && !String.IsNullOrEmpty(phase2ResultBody?.statusMessage))
            {
                return new ProcessorCreateTransactionResponse(phase2ResultBody?.statusMessage, phase2ResultBody?.statusCode);
            }
            else
            {
                return new ProcessorCreateTransactionResponse(Messages.CannotGetErrorCodeFromResponse, RejectionReasonEnum.Unknown, phase2ResultBody.statusCode);
            }
        }

        public async Task<ProcessorTransmitTransactionsResponse> TransmitTransactions(ProcessorTransmitTransactionsRequest transmitTransactionsRequest)
        {
            ///cal SHVA TRANSNISSION TODO
            //ShvaTerminalSettings shvaParameters = transmitTransactionsRequest.ProcessorSettings as ShvaTerminalSettings;

            //if (shvaParameters == null)
            //{
            //    throw new ArgumentNullException("ShvaTerminalSettings (at transmitTransactionsRequest.ProcessorSettings) is required");
            //}

            //var res = new ShvaTransmissionResponse();

            //var tranEMV = new TransEMVRequestBody();
            //tranEMV.UserName = shvaParameters.UserName;
            //tranEMV.Password = shvaParameters.Password;
            //tranEMV.MerchantNumber = shvaParameters.MerchantNumber;
            //tranEMV.DATA = string.Join(";", transmitTransactionsRequest.TransactionIDs);

            //var result = await this.DoRequest(tranEMV, TransEMVUrl, transmitTransactionsRequest.CorrelationId, HandleIntegrationMessage);

            //var transResultBody = (TransEMVResponseBody)result?.Body?.Content;

            //if (transResultBody == null)
            //{
            //    throw new IntegrationException("Empty transmission response", null); // TODO: integration message ID
            //}

            //res.ProcessorCode = transResultBody.TransEMVResult;
            //res.FailedTransactions = transResultBody.BadTrans?.Split(new string[] { ";", " " }, StringSplitOptions.RemoveEmptyEntries);
            //res.TransmissionReference = transResultBody.RefNumber;
            //res.Report = transResultBody.Report;
            //res.TotalCreditTransSum = transResultBody.TotalCreditTransSum;
            //res.TotalDebitTransSum = transResultBody.TotalDebitTransSum;
            //res.TotalXML = transResultBody.TotalXML;

            // TODO: failed case (?)

            return null;
        }

        //protected async Task<Envelope> DoRequest(object request, string soapAction, string correlationId, Func<IntegrationMessage, Task> handleIntegrationMessage = null)
        //{
        //    var soap = new Envelope
        //    {
        //        Body = new Body
        //        {
        //            Content = request
        //        },
        //    };

        //    Envelope svcRes = null;

        //    string requestUrl = null;
        //    string requestStr = null;
        //    string responseStr = null;
        //    string responseStatusStr = null;

        //    var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

        //    try
        //    {
        //        svcRes = await this.apiClient.PostXml<Envelope>(configuration.BaseUrl, string.Empty, soap, () => BuildHeaders(soapAction),
        //            (url, request) =>
        //            {
        //                requestStr = request;
        //                requestUrl = url;
        //            },
        //            (response, responseStatus, responseHeaders) =>
        //            {
        //                responseStr = response;
        //                responseStatusStr = responseStatus.ToString();
        //            });
        //    }
        //    catch (Exception ex)
        //    {
        //        this.logger.LogError(ex, $"Nayax integration request failed ({integrationMessageId}): {ex.Message}");

        //        throw new IntegrationException("Shva integration request failed", integrationMessageId);
        //    }
        //    finally
        //    {
        //        if (handleIntegrationMessage != null)
        //        {
        //            IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, integrationMessageId, correlationId);

        //            integrationMessage.Request = requestStr;
        //            integrationMessage.Response = responseStr;
        //            integrationMessage.ResponseStatus = responseStatusStr;
        //            integrationMessage.Address = requestUrl;
        //            integrationMessage.Action = soapAction;

        //            await handleIntegrationMessage?.Invoke(integrationMessage);
        //        }
        //    }

        //    return svcRes;
        //}

        //private async Task<NameValueCollection> BuildHeaders(string soapAction)
        //{
        //    NameValueCollection headers = new NameValueCollection();

        //    return await Task.FromResult(headers);
        //}

        
        private async Task HandleIntegrationMessage(IntegrationMessage msg)
        {
            await integrationRequestLogStorageService.Save(msg);
        }

        private async Task<NameValueCollection> BuildHeaders()
        {
            NameValueCollection headers = new NameValueCollection();
            if (configuration != null)
            {
                headers.Add("x-api-key",  configuration.APIKey);
            }
            return headers;
        }
    }
}
