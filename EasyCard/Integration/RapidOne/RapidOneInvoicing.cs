using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RapidOne.Configuration;
using RapidOne.Converters;
using RapidOne.Models;
using Shared.Helpers;
using Shared.Integration;
using Shared.Integration.Exceptions;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace RapidOne
{
    public class RapidOneInvoicing : IInvoicing
    {
        private const string documentProduce = "/gateway/financialDocuments";
        private const string ResponseTokenHeader = "Authorization";
        private readonly IIntegrationRequestLogStorageService storageService;
        private readonly IWebApiClient apiClient;
        private readonly RapidOneGlobalSettings configuration;
        private readonly ILogger logger;

        public RapidOneInvoicing(
            IWebApiClient apiClient,
            IOptions<RapidOneGlobalSettings> configuration,
            ILogger<RapidOneInvoicing> logger,
            IIntegrationRequestLogStorageService storageService)
        {
            this.configuration = configuration.Value;

            this.storageService = storageService;

            this.apiClient = apiClient;

            this.logger = logger;
        }

        public async Task<InvoicingCreateDocumentResponse> CreateDocument(InvoicingCreateDocumentRequest documentCreationRequest)
        {
            var terminal = documentCreationRequest.InvoiceingSettings as RapidOneTerminalSettings;

            var json = RapidInvoiceConverter.GetInvoiceCreateDocumentRequest(documentCreationRequest);
            json.BranchId = terminal.Branch;
            json.Company = terminal.Company;
            json.DepartmentId = terminal.Department;
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            NameValueCollection headers = GetAuthorizedHeaders(terminal.BaseUrl, terminal.Token, integrationMessageId, documentCreationRequest.CorrelationId);

            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;

            try
            {
                var svcRes = await this.apiClient.Post<IEnumerable<DocumentItemModel>>(terminal.BaseUrl, documentProduce, json, () => Task.FromResult(headers),
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

                var firstResult = svcRes.FirstOrDefault();
                return new InvoicingCreateDocumentResponse
                {
                    DocumentNumber = firstResult?.DocEntry.ToString(),
                    DownloadUrl = firstResult?.Url,
                    CopyDonwnloadUrl = firstResult?.Url,
                    ExternalSystemData = JObject.FromObject(new RapidOneCreateDocumentResponse(svcRes))
                };
            }
            catch (WebApiClientErrorException wex)
            {
                this.logger.LogError(wex, $"RapidOne integration request failed. {wex.Message} ({integrationMessageId}). CorrelationId: {documentCreationRequest.CorrelationId}");

                try
                {
                    var errResp = JsonConvert.DeserializeObject<RapidOneCreateDocumentErrorResponse>(wex.Response);

                    return new InvoicingCreateDocumentResponse
                    {
                        Success = false,
                        ErrorMessage = errResp.Error,
                        OriginalHttpResponseCode = (int)wex.StatusCode
                    };
                }
                catch (Exception)
                {
                    throw new IntegrationException("RapidOne integration request failed", integrationMessageId);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"RapidOne integration request failed. {ex.Message} ({integrationMessageId}). CorrelationId: {documentCreationRequest.CorrelationId}");

                throw new IntegrationException("RapidOne integration request failed", integrationMessageId);
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
        }

        public async Task<IEnumerable<string>> GetDownloadUrls(JObject externalSystemData, object invoiceingSettings)
        {
            var terminal = invoiceingSettings as RapidOneTerminalSettings;

            RapidOneCreateDocumentResponse documentResponse = externalSystemData.ToObject<RapidOneCreateDocumentResponse>();

            // TODO: get temporary url from R1

            return documentResponse.Documents.Select(d => d.Url);
        }

        private NameValueCollection GetAuthorizedHeaders(string BaseUrl, string Token, string integrationMessageId, string correlationId)
        {
            try
            {
                NameValueCollection headers = new NameValueCollection();
                headers.Add(ResponseTokenHeader, string.Format("ExternalClient {0}", Token));
                return headers;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"RapidOne integration request failed: failed to get token: {ex.Message} ({integrationMessageId}). CorrelationId: {correlationId}");

                throw new IntegrationException("RapidOne integration request failed: failed to get token", integrationMessageId);
            }
        }
    }
}
