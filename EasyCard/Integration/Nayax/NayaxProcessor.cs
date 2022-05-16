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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nayax
{
    public class NayaxProcessor : IProcessor
    {
        private const string Phase1Url = "doTransactionPhase1";
        private const string Phase2Url = "doTransactionPhase2";
        private const string DoPeriodic = "doPeriodic";
        private const string Pair = "pair";
        private const string Authenticate = "authenticate";
        private const string GetDetails = "getStatus";
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
        public async Task<ProcessorPreCreateTransactionResponse> PreCreateTransaction(ProcessorCreateTransactionRequest paymentTransactionRequest)
        {
            NayaxTerminalSettings nayaxParameters = paymentTransactionRequest.PinPadProcessorSettings as NayaxTerminalSettings;

            if (nayaxParameters == null)
            {
                throw new ArgumentNullException("NayaxTerminalSettings (at paymentTransactionRequest.ProcessorSettings) is required");
            }

            var phas1Req = nayaxParameters.GetPhase1RequestBody(configuration);
            ObjectInPhase1RequestParams params2 = paymentTransactionRequest.GetObjectInPhase1RequestParams();

            phas1Req.paramss[1] = params2;

            Phase1ResponseBody phase1ResultBody = null;

            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            try
            {

                phase1ResultBody = await this.apiClient.Post<Models.Phase1ResponseBody>(configuration.BaseUrl, Phase1Url, phas1Req, BuildHeaders,
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
                this.logger.LogError(ex, $"Nayax integration request failed ({integrationMessageId}): {ex.Message}");

                throw new IntegrationException("Nayax integration request failed", integrationMessageId);
            }
            finally
            {
                IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, paymentTransactionRequest.PaymentTransactionID, integrationMessageId, paymentTransactionRequest.CorrelationId);

                //Do not expose credit card and cvv numbers in log
                //requestStr = Regex.Replace(requestStr, "\\<clientInputPan\\>\\d{9,16}\\</clientInputPan\\>", "<clientInputPan>****************</clientInputPan>");
                //requestStr = Regex.Replace(requestStr, "\\<cvv2\\>\\d{3,4}\\</cvv2\\>", "<cvv2>***</cvv2>");

                integrationMessage.Request = requestStr;
                integrationMessage.Response = responseStr;
                integrationMessage.ResponseStatus = responseStatusStr;
                integrationMessage.Address = requestUrl;

                await integrationRequestLogStorageService.Save(integrationMessage);
            }


            if (phase1ResultBody == null)
            {
                return new ProcessorPreCreateTransactionResponse(" ", RejectionReasonEnum.Unknown, string.Empty);
            }

            if (!Int32.TryParse(phase1ResultBody.statusCode, out var statusPreCreate))
            {
                // return failed response
                return new ProcessorPreCreateTransactionResponse("Status code is not valid", RejectionReasonEnum.Unknown, string.Empty);
            }


            if (phase1ResultBody.IsSuccessful())
            {
                return phase1ResultBody.GetProcessorPreTransactionResponse(paymentTransactionRequest.PinPadTransactionID);
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

            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            Phase2ResponseBody phase2ResultBody;

            try
            {

                phase2ResultBody = await this.apiClient.Post<Models.Phase2ResponseBody>(configuration.BaseUrl, Phase2Url, phase2Req, BuildHeaders,
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
                this.logger.LogError(ex, $"Nayax integration request failed ({integrationMessageId}): {ex.Message}");

                throw new IntegrationException("Nayax integration request failed", integrationMessageId);
            }
            finally
            {
                IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, paymentTransactionRequest.PaymentTransactionID, integrationMessageId, paymentTransactionRequest.CorrelationId);

                //Do not expose credit card and cvv numbers in log
                //requestStr = Regex.Replace(requestStr, "\\<clientInputPan\\>\\d{9,16}\\</clientInputPan\\>", "<clientInputPan>****************</clientInputPan>");
                //requestStr = Regex.Replace(requestStr, "\\<cvv2\\>\\d{3,4}\\</cvv2\\>", "<cvv2>***</cvv2>");

                integrationMessage.Request = requestStr;
                integrationMessage.Response = responseStr;
                integrationMessage.ResponseStatus = responseStatusStr;
                integrationMessage.Address = requestUrl;

                await integrationRequestLogStorageService.Save(integrationMessage);
            }

            if (phase2ResultBody == null)
            {
                // return failed response
                return new ProcessorCreateTransactionResponse(Messages.EmptyResponse, RejectionReasonEnum.Unknown, string.Empty);
            }

            if (!Int32.TryParse(phase2ResultBody.statusCode, out var statusCreate))
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
                return new ProcessorCreateTransactionResponse(phase2ResultBody?.statusMessage, phase2ResultBody?.statusCode, Int32.Parse(phase2ResultBody?.statusCode));
            }
            else
            {
                return new ProcessorCreateTransactionResponse(Messages.CannotGetErrorCodeFromResponse, RejectionReasonEnum.Unknown, phase2ResultBody.statusCode, Int32.Parse(phase2ResultBody?.statusCode));
            }
        }

        public Task<ProcessorTransmitTransactionsResponse> TransmitTransactions(ProcessorTransmitTransactionsRequest transmitTransactionsRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<PairResponse> PairDevice(PairRequest pairRequest)
        {
            //pinpadProcessorSettings = processorResolver.GetProcessorTerminalSettings(terminalPinpadProcessor, terminalPinpadProcessor.Settings);
            //NayaxTerminalSettings nayaxParameters = paymentTransactionRequest.PinPadProcessorSettings as NayaxTerminalSettings;

            if (pairRequest == null)
            {
                throw new ArgumentNullException("PairDevice request is required");
            }

            var pairReq = EMVDealHelper.GetPairRequestBody(configuration, pairRequest.posName, pairRequest.terminalID);

            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            PairResponseBody pairResultBody = null;

            try
            {
                pairResultBody = await this.apiClient.Post<Models.PairResponseBody>(configuration.BaseUrl, Pair, pairReq, BuildHeaders,
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
                this.logger.LogError(ex, $"Nayax integration request failed ({integrationMessageId}): {ex.Message}");

                throw new IntegrationException("Nayax integration request failed", integrationMessageId);
            }
            finally
            {
                IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, pairRequest.terminalID, integrationMessageId, pairRequest.CorrelationId);

                integrationMessage.Request = requestStr;
                integrationMessage.Response = responseStr;
                integrationMessage.ResponseStatus = responseStatusStr;
                integrationMessage.Address = requestUrl;

                await integrationRequestLogStorageService.Save(integrationMessage);
            }


            if (pairResultBody == null)
            {
                return new PairResponse(Messages.EmptyResponse, string.Empty);
            }

            if (!Int32.TryParse(pairResultBody.statusCode, out var statusPair))
            {
                return new PairResponse(Messages.StatusCodeIsNotValid, pairResultBody.statusCode);
            }

            if (pairResultBody.IsSuccessful())
            {
                return new PairResponse();
            }
            else if (!String.IsNullOrEmpty(pairResultBody?.statusCode) && !String.IsNullOrEmpty(pairResultBody?.statusMessage))
            {
                return new PairResponse(pairResultBody?.statusMessage, pairResultBody?.statusCode);
            }
            else
            {
                return new PairResponse(Messages.CannotGetErrorCodeFromResponse, pairResultBody.statusCode);
            }
        }

        public async Task<PairResponse> AuthenticateDevice(AuthenticateRequest authRequest)
        {
            //pinpadProcessorSettings = processorResolver.GetProcessorTerminalSettings(terminalPinpadProcessor, terminalPinpadProcessor.Settings);
            //NayaxTerminalSettings nayaxParameters = paymentTransactionRequest.PinPadProcessorSettings as NayaxTerminalSettings;

            if (authRequest == null)
            {
                throw new ArgumentNullException("AuthenticaterDevice request is required");
            }

            var authReq = EMVDealHelper.GetAuthRequestBody(configuration, authRequest.OTP, authRequest.terminalID);
            var authReqResult = await this.apiClient.Post<Models.AuthResponseBody>(configuration.BaseUrl, Authenticate, authReq, BuildHeaders);

            var authResultBody = authReqResult as AuthResponseBody;

            if (authResultBody == null)
            {
                return new PairResponse(Messages.EmptyResponse, string.Empty);
            }
            int statusPair;
            if (!Int32.TryParse(authResultBody.statusCode, out statusPair))
            {
                return new PairResponse(Messages.StatusCodeIsNotValid, authResultBody.statusCode);
            }


            if (authResultBody.IsSuccessful())
            {
                return new PairResponse();
            }
            else if (!String.IsNullOrEmpty(authResultBody?.statusCode) && !String.IsNullOrEmpty(authResultBody?.statusMessage))
            {
                return new PairResponse(authResultBody?.statusMessage, authResultBody?.statusCode);
            }
            else
            {
                return new PairResponse(Messages.CannotGetErrorCodeFromResponse, authResultBody.statusCode);
            }
        }

        public async Task<bool> TestConnection(AuthRequest authRequest)
        {
            //pinpadProcessorSettings = processorResolver.GetProcessorTerminalSettings(terminalPinpadProcessor, terminalPinpadProcessor.Settings);
            //NayaxTerminalSettings nayaxParameters = paymentTransactionRequest.PinPadProcessorSettings as NayaxTerminalSettings;

            if (authRequest == null)
            {
                throw new ArgumentNullException("TestConnection request is required");
            }

            var authReq = EMVDealHelper.GetAutheRequestBody(configuration, authRequest.terminalID);
            try
            {
                var response = this.apiClient.Post<HttpResponseMessage>(configuration.BaseUrl, GetDetails, authReq, BuildHeaders);
                var responseRes = response.Result.Content.ReadAsStringAsync().Result;
                return (!string.IsNullOrEmpty(responseRes) && (responseRes.ToLower().Contains("ashraitready") || responseRes.ToLower().Contains("ashraitreadyonline")));
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<ProcessorUpdateParamteresResponse> ParamsUpdateTransaction(ProcessorUpdateParametersRequest updateParamRequest)
        {
            var res = new ProcessorCreateTransactionResponse();
            
            if (updateParamRequest == null)
            {
                throw new ArgumentNullException("NayaxTerminalSettings is required");
            }

            var nayaxParameters = updateParamRequest.ProcessorSettings as NayaxTerminalSettings;

            if (nayaxParameters == null)
            {
                throw new ArgumentNullException("NayaxTerminalSettings is required");
            }

            var updateParamsReq = nayaxParameters.GetDoPeriodicRequest(configuration);

            DoPeriodicResultBody updateParamResultBody;
            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);
            try
            {

                updateParamResultBody = await this.apiClient.Post<Models.DoPeriodicResultBody>(configuration.BaseUrl, DoPeriodic, updateParamsReq, BuildHeaders,
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
                return new ProcessorUpdateParamteresResponse()
                {
                    Code = updateParamResultBody.statusCode,
                    Success = updateParamResultBody.statusCode == 0
                };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Nayax integration request failed ({integrationMessageId}): {ex.Message}");
               // return new ProcessorUpdateParamteresResponse()
               // {
               //     Success = false;
               // };
                throw new IntegrationException("Nayax integration request failed", integrationMessageId);
            }
            finally
            {
                IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, nayaxParameters.TerminalID, integrationMessageId, updateParamRequest.CorrelationId);

                integrationMessage.Request = requestStr;
                integrationMessage.Response = responseStr;
                integrationMessage.ResponseStatus = responseStatusStr;
                integrationMessage.Address = requestUrl;

                await integrationRequestLogStorageService.Save(integrationMessage);
            }
        }

        public Task<IEnumerable<IntegrationMessage>> GetStorageLogs(string entityID)
        {
            return integrationRequestLogStorageService.GetAll(entityID);
        }

        private async Task<NameValueCollection> BuildHeaders()
        {
            NameValueCollection headers = new NameValueCollection();
            if (configuration != null)
            {
                headers.Add("x-api-key", configuration.APIKey);
            }
            return await Task.FromResult(headers);
        }
    }
}
