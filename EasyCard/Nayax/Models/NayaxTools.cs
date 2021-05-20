using System;
using System.Collections.Generic;
using System.Text;

namespace Nayax.Models
{
    public class NayaxTools
    {
        private const string Pair = "pair";
        private const string Authenticate = "authenticate";
        /*
        public async Task<Object> PreCreateTransaction(ProcessorCreateTransactionRequest paymentTransactionRequest)
        {
            NayaxTerminalSettings nayaxParameters = paymentTransactionRequest.PinPadProcessorSettings as NayaxTerminalSettings;

            if (nayaxParameters == null)
            {
                throw new ArgumentNullException("NayaxTerminalSettings (at paymentTransactionRequest.ProcessorSettings) is required");
            }

            var phas1Req = nayaxParameters.GetPhase1RequestBody(configuration, paymentTransactionRequest.EasyCardTerminalID);
            ObjectInPhase1RequestParams params2 = paymentTransactionRequest.GetObjectInPhase1RequestParams();

            phas1Req.paramss[1] = params2;
            //client.Timeout = TimeSpan.FromSeconds(30); TODO timeout for 30 minutes
            var phase1ReqResult = await this.apiClient.Post<Models.Phase1ResponseBody>(configuration.BaseUrl, Phase1Url, phas1Req, BuildHeaders);//this.DoRequest(phas1Req, Phase1Url, paymentTransactionRequest.CorrelationId, HandleIntegrationMessage);

            var phase1ResultBody = phase1ReqResult as Phase1ResponseBody;

            if (phase1ResultBody == null)
            {
                return new ProcessorPreCreateTransactionResponse(" ", RejectionReasonEnum.Unknown, string.Empty);
            }
            int statusPreCreate;
            if (!Int32.TryParse(phase1ResultBody.statusCode, out statusPreCreate))
            {
                // return failed response
                return new ProcessorPreCreateTransactionResponse("Status code is not valid", RejectionReasonEnum.Unknown, string.Empty);
            }
            // end request


            if (phase1ResultBody.IsSuccessful())
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
        */
        //TODO!!!!!
    }
}
