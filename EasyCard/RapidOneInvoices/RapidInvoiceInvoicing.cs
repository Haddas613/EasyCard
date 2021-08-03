using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RapidOneInvoices.Configuration;
using RapidOneInvoices.Converters;
using RapidOneInvoices.Models;
using Shared.Helpers;
using Shared.Integration;
using Shared.Integration.Exceptions;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace RapidOneInvoices
{
    public class RapidInvoiceInvoicing : IInvoicing
    {
        private const string documentProduce = "/gateway/financialDocuments";
        private const string ResponseTokenHeader = "ExternalClient";
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
            
            var terminal = documentCreationRequest.InvoiceingSettings as RapidInvoiceTerminalSettings;

            var json = RapidInvoiceConverter.GetInvoiceCreateDocumentRequest(documentCreationRequest);
            json.BranchId = Int32.Parse(terminal.Branch);
            json.Company = terminal.Company;
            json.DepartmentId = Int32.Parse(terminal.Department);
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            NameValueCollection headers = GetAuthorizedHeaders(terminal.BaseUrl, terminal.Token, integrationMessageId, documentCreationRequest.CorrelationId);

            List<DocumentItemModel> svcRes = null;
            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;

            try
            {
                svcRes = await this.apiClient.Post<List<DocumentItemModel>>(terminal.BaseUrl, documentProduce, json, () => Task.FromResult(headers),
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
                this.logger.LogError(ex, $"Rapid invoice integration request failed. {ex.Message} ({integrationMessageId}). CorrelationId: {documentCreationRequest.CorrelationId}");

                throw new IntegrationException("Rapid invoice integration request failed", integrationMessageId);
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
                DocumentNumber = svcRes[0]?.docEntry.ToString(),
                DownloadUrl = svcRes[0]?.url,
                CopyDonwnloadUrl = svcRes[0]?.url
            };

           //if (svcRes == null || svcRes.Count<1 || string.IsNullOrWhiteSpace(svcRes[0].url) || svcRes[0].docEntry <= 0)
           //{
           //    response.Success = false;
           //    response.ErrorMessage = svcRes.Message ?? svcRes.Error;
           //}

            return response;
        }

        private NameValueCollection GetAuthorizedHeaders(string BaseUrl, string Token, string integrationMessageId, string correlationId)
        {
            try
            {
                NameValueCollection headers = new NameValueCollection();
                headers.Add(ResponseTokenHeader, Token);
                return headers;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Rapid invoice integration request failed: failed to get token: {ex.Message} ({integrationMessageId}). CorrelationId: {correlationId}");

                throw new IntegrationException("Rapid invoice integration request failed: failed to get token", integrationMessageId);
            }
        }
        



    }
}
