using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RapidOneInvoices.Configuration;
using RapidOneInvoices.Converters;
using Shared.Helpers;
using Shared.Integration;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models.Invoicing;
using System;
using System.Threading.Tasks;

namespace RapidOneInvoices
{
    public class RapidInvoiceInvoicing : IInvoicing
    {
        private readonly IIntegrationRequestLogStorageService storageService;
        private readonly IWebApiClient apiClient;
        private readonly RapidInvoiceGlobalSettings configuration;
        private readonly ILogger logger;

        public RapidInvoiceInvoicing(
            IWebApiClient apiClient,
            IOptions<RapidInvoiceGlobalSettings> configuration,
            ILogger<RapidInvoiceInvoicing> logger,
            IIntegrationRequestLogStorageService storageService)
        {
            this.configuration = configuration.Value;

            this.storageService = storageService;

            this.apiClient = apiClient;

            this.logger = logger;
        }
        public async Task<InvoicingCreateDocumentResponse> CreateDocument(InvoicingCreateDocumentRequest documentCreationRequest)
        {
            /*
            var terminal = documentCreationRequest.InvoiceingSettings as RapidInvoiceTerminalSettings;

            var json = RapidInvoiceConverter.GetInvoiceCreateDocumentRequest(documentCreationRequest);
            //json.BranchId = Int32.Parse(terminal.Branch);
            //json.Company = terminal.Company;
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            NameValueCollection headers = GetAuthorizedHeaders(terminal.UserName, terminal.Password, integrationMessageId, documentCreationRequest.CorrelationId);

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
            */
            return null;
        }

    }
}
