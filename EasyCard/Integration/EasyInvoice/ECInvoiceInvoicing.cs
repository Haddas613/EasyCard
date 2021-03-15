using EasyInvoice.Converters;
using EasyInvoice.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Helpers;
using Shared.Integration;
using Shared.Integration.Exceptions;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace EasyInvoice
{
    public class ECInvoiceInvoicing : IInvoicing
    {
        private const string ResponseTokenHeader = "X-AUTH-TOKEN";

        private readonly IIntegrationRequestLogStorageService storageService;
        private readonly IWebApiClient apiClient;
        private readonly EasyInvoiceGlobalSettings configuration;
        private readonly ILogger logger;

        public ECInvoiceInvoicing(
            IWebApiClient apiClient,
            IOptions<EasyInvoiceGlobalSettings> configuration,
            ILogger<ECInvoiceInvoicing> logger,
            IIntegrationRequestLogStorageService storageService)
        {
            this.configuration = configuration.Value;

            this.storageService = storageService;

            this.apiClient = apiClient;

            this.logger = logger;
        }

        public async Task<InvoicingCreateDocumentResponse> CreateDocument(InvoicingCreateDocumentRequest documentCreationRequest)
        {
            var terminal = documentCreationRequest.InvoiceingSettings as EasyInvoiceTerminalSettings;

            var json = ECInvoiceConverter.GetInvoiceCreateDocumentRequest(documentCreationRequest);
            json.KeyStorePassword = terminal.KeyStorePassword;

            NameValueCollection headers = new NameValueCollection();

            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            try
            {
                var loginRequest = new { email = terminal.UserName, password = terminal.Password };

                var loginres = apiClient.PostRawWithHeaders(this.configuration.BaseUrl, "/api/v1/login", JsonConvert.SerializeObject(loginRequest), "application/json").Result;

                var authToken = loginres.ResponseHeaders.AllKeys.Any(k => k == ResponseTokenHeader) ? loginres.ResponseHeaders[ResponseTokenHeader] : null;

                headers.Add(ResponseTokenHeader, authToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"EasyInvoice integration request failed: failed to get token: {ex.Message} ({integrationMessageId}). CorrelationId: {documentCreationRequest.CorrelationId}");

                throw new IntegrationException("EasyInvoice integration request failed: failed to get token", integrationMessageId);
            }

            ECInvoiceDocumentResponse svcRes = null;
            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;

            try
            {
                headers.Add("Accept-language", "he"); // TODO: get language from options

                svcRes = await this.apiClient.Post<ECInvoiceDocumentResponse>(this.configuration.BaseUrl, "/api/v1/docs", json, () => Task.FromResult(headers),
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
                this.logger.LogError(ex, $"EasyInvoice integration request failed. {ex.Message} ({integrationMessageId}). CorrelationId: {documentCreationRequest.CorrelationId}");

                throw new IntegrationException("EasyInvoice integration request failed", integrationMessageId);
            }
            finally
            {
                IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, integrationMessageId, documentCreationRequest.CorrelationId)
                {
                    Request = requestStr,
                    Response = responseStr,
                    ResponseStatus = responseStatusStr,
                    Address = requestUrl
                };

                await storageService.Save(integrationMessage);
            }

            var response = new InvoicingCreateDocumentResponse
            {
                DocumentNumber = svcRes.DocumentNumber?.ToString(),
                DownloadUrl = svcRes.DocumentUrl,
                CopyDonwnloadUrl = svcRes.DocumentCopyUrl
            };

            if (svcRes.Errors?.Count > 0 || !string.IsNullOrWhiteSpace(svcRes.Error) || string.IsNullOrWhiteSpace(svcRes.DocumentUrl) || svcRes.DocumentNumber.GetValueOrDefault() <= 0)
            {
                response.Success = false;
                response.ErrorMessage = svcRes.Message ?? svcRes.Error;
            }

            return response;
        }
    }
}
