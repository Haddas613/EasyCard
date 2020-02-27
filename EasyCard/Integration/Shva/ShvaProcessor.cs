using Microsoft.Extensions.Logging;
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
            // TODO: implement mapping
            var inValue = new AshStartRequestBody();
            inValue.UserName = configuration.UserName;
            inValue.Password = configuration.Password;
            inValue.MerchantNumber = paymentTransactionRequest.TerminalReference;
            // TODO: other fields
           


            var result = await this.DoRequest(inValue, "http://shva.co.il/xmlwebservices/AshStart", messageId, correlationId, handleIntegrationMessage);

            var resultBody = (AshStartResponseBody)result?.Body?.Content;

            if (resultBody == null) return null;

            // TODO: implement mapping
            var res = new ExternalPaymentTransactionResponse();
            res.TransactionReference = resultBody.pinpad.track2; // TODO: use right fields

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
