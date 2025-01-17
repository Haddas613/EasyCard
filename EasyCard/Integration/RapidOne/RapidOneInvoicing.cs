﻿using Microsoft.Extensions.Logging;
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
        private const string getDownloadUrl = "/gateway/getDownloadUrl";
        private const string getCompaniesUrl = "/gateway/companies";
        private const string getBranchesUrl = "/gateway/branches";
        private const string getDepartmentsUrl = "/gateway/departments";
        private const string getItemsUrl = "/gateway/items";
        private const string getItemCategoriesUrl = "/gateway/itemcategories";
        private const string createCustomerUrl = "/gateway/getorcreate";
        private const string createItemUrl = "/gateway/item";
        private const string createItemCategoryUrl = "/gateway/itemcategory";




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

            if (string.IsNullOrWhiteSpace(documentCreationRequest.DealDetails?.ConsumerExternalReference))
            {
                var cres = await CreateConsumerOrGetExisting(new CreateConsumerRequest {
                     InvoiceingSettings = terminal,
                     ConsumerName = documentCreationRequest.ConsumerName ?? documentCreationRequest.DealDetails?.ConsumerName,
                     Email = documentCreationRequest.DealDetails?.ConsumerEmail,
                     CellPhone = documentCreationRequest.DealDetails?.ConsumerPhone,
                     NationalID = documentCreationRequest.ConsumerNationalID ?? documentCreationRequest.DealDetails?.ConsumerNationalID,
                });

                if (!string.IsNullOrWhiteSpace(cres.ConsumerReference))
                {
                    if (documentCreationRequest.DealDetails == null)
                    {
                        documentCreationRequest.DealDetails = new Shared.Integration.Models.DealDetails();
                    }

                    documentCreationRequest.DealDetails.ConsumerExternalReference = cres.ConsumerReference;
                }
                else
                {
                    throw new IntegrationException("Cannot create RapidOne document because CardCode is empty", null);
                }
            }

            var json = RapidInvoiceConverter.GetInvoiceCreateDocumentRequest(documentCreationRequest, terminal);
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
                    DocumentNumber = GetDocumentNumber(svcRes),
                    DownloadUrl = null,
                    CopyDonwnloadUrl = null,
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
                IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, documentCreationRequest.InvoiceID, integrationMessageId, documentCreationRequest.CorrelationId)
                {
                    Request = requestStr,
                    Response = responseStr,
                    ResponseStatus = responseStatusStr,
                    Address = requestUrl
                };

                await storageService.Save(integrationMessage);
            }
        }

        public async Task<IEnumerable<string>> GetDownloadUrls(JObject externalSystemData, object invoiceingSettings, string language = null)
        {
            var terminal = invoiceingSettings as RapidOneTerminalSettings;

            RapidOneCreateDocumentResponse documentResponse = externalSystemData.ToObject<RapidOneCreateDocumentResponse>();
            NameValueCollection headers = GetAuthorizedHeaders(terminal.BaseUrl, terminal.Token, null, null);

            if (language != null)
            {
                foreach(var doc in documentResponse.Documents)
                {
                    doc.Lang = language;
                }
            }

            try
            {
                var svcRes = await this.apiClient.Post<IEnumerable<DocumentItemModel>>(terminal.BaseUrl, getDownloadUrl, documentResponse.Documents, () => Task.FromResult(headers));

                var rs = svcRes.Select(d => d.Url);

                return rs;
               
            }
            catch (WebApiClientErrorException wex)
            {
                this.logger.LogError(wex, $"RapidOne integration request failed. {wex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"RapidOne integration request failed. {ex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
        }

        public async Task<IEnumerable<CompanyDto>> GetCompanies(string baseurl, string token)
        {
            NameValueCollection headers = GetAuthorizedHeaders(baseurl, token, null, null);

            try
            {
                var res =  await this.apiClient.Get<RapidOneSummaryResponse<CompanyDto>>(baseurl, getCompaniesUrl, null, () => Task.FromResult(headers));
                return res?.Data ?? Enumerable.Empty<CompanyDto>();
            }
            catch (WebApiClientErrorException wex)
            {
                this.logger.LogError(wex, $"RapidOne integration request failed. {wex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"RapidOne integration request failed. {ex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
        }

        public async Task<IEnumerable<BranchDto>> GetBranches(string baseurl, string token)
        {
            NameValueCollection headers = GetAuthorizedHeaders(baseurl, token, null, null);

            try
            {
                var res = await this.apiClient.Get<RapidOneSummaryResponse<BranchDto>>(baseurl, getBranchesUrl, null, () => Task.FromResult(headers));
                return res?.Data ?? Enumerable.Empty<BranchDto>();
            }
            catch (WebApiClientErrorException wex)
            {
                this.logger.LogError(wex, $"RapidOne integration request failed. {wex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"RapidOne integration request failed. {ex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
        }

        public async Task<IEnumerable<ItemWithPricesDto>> GetItems(string baseurl, string token)
        {
            NameValueCollection headers = GetAuthorizedHeaders(baseurl, token, null, null);

            try
            {
                var res = await this.apiClient.Get<RapidOneSummaryResponse<ItemWithPricesDto>>(baseurl, getItemsUrl, null, () => Task.FromResult(headers));
                return res?.Data ?? Enumerable.Empty<ItemWithPricesDto>();
            }
            catch (WebApiClientErrorException wex)
            {
                this.logger.LogError(wex, $"RapidOne integration request failed. {wex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"RapidOne integration request failed. {ex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
        }

        public async Task<ItemDto> CreateItem(string baseurl, string token, ItemDto model)
        {
            NameValueCollection headers = GetAuthorizedHeaders(baseurl, token, null, null);

            try
            {
                var res = await this.apiClient.Post<ItemDto>(baseurl, createItemUrl, model, () => Task.FromResult(headers));
                return res;
            }
            catch (WebApiClientErrorException wex)
            {
                this.logger.LogError(wex, $"RapidOne integration request failed. {wex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"RapidOne integration request failed. {ex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
        }

        public async Task<ItemCategoryDto> CreateItemCategory(string baseurl, string token, ItemCategoryDto model)
        {
            NameValueCollection headers = GetAuthorizedHeaders(baseurl, token, null, null);

            try
            {
                var res = await this.apiClient.Post<ItemCategoryDto>(baseurl, createItemCategoryUrl, model, () => Task.FromResult(headers));
                return res;
            }
            catch (WebApiClientErrorException wex)
            {
                this.logger.LogError(wex, $"RapidOne integration request failed. {wex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"RapidOne integration request failed. {ex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
        }

        public async Task<IEnumerable<ItemCategoryDto>> GetItemCategories(string baseurl, string token)
        {
            NameValueCollection headers = GetAuthorizedHeaders(baseurl, token, null, null);

            try
            {
                var res = await this.apiClient.Get<IEnumerable<ItemCategoryDto>> (baseurl, getItemCategoriesUrl, null, () => Task.FromResult(headers));
                return res;
            }
            catch (WebApiClientErrorException wex)
            {
                this.logger.LogError(wex, $"RapidOne integration request failed. {wex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"RapidOne integration request failed. {ex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
        }

        public async Task<IEnumerable<DepartmentDto>> GetDepartments(string baseurl, string token, int? branchId = null)
        {
            NameValueCollection headers = GetAuthorizedHeaders(baseurl, token, null, null);

            try
            {
                var res =  await this.apiClient.Get<RapidOneSummaryResponse<DepartmentDto>>(baseurl, getDepartmentsUrl, new { branchId }, () => Task.FromResult(headers));
                return res?.Data ?? Enumerable.Empty<DepartmentDto>();
            }
            catch (WebApiClientErrorException wex)
            {
                this.logger.LogError(wex, $"RapidOne integration request failed. {wex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"RapidOne integration request failed. {ex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
        }

        public Task<IEnumerable<IntegrationMessage>> GetStorageLogs(string entityID)
        {
            return storageService.GetAll(entityID);
        }

        public async Task<CreateConsumerResponse> CreateConsumerOrGetExisting(CreateConsumerRequest consumerRequest)
        {
            var terminal = consumerRequest.InvoiceingSettings as RapidOneTerminalSettings;

            NameValueCollection headers = GetAuthorizedHeaders(terminal.BaseUrl, terminal.Token, null, null);

            var model = RapidInvoiceConverter.GetCustomerDto(consumerRequest);

            try
            {
                var res = await this.apiClient.Post<CreateCustomerResult>(terminal.BaseUrl, createCustomerUrl, model, () => Task.FromResult(headers));
                return new CreateConsumerResponse { Success = res.Succeeded, ConsumerReference = res.CardCode, ErrorMessage = res.ErrorMessage, Origin = terminal.BaseUrl };
            }
            catch (WebApiClientErrorException wex)
            {
                this.logger.LogError(wex, $"RapidOne integration request failed. {wex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"RapidOne integration request failed. {ex.Message}");

                throw new IntegrationException("RapidOne integration request failed", null);
            }
        }

        public bool CanCreateConsumer()
        {
            return true;
        }

        public bool CanCancelDocument()
        {
            return false;
        }

        public Task<InvoicingCancelDocumentResponse> CancelDocument(InvoicingCancelDocumentRequest documentCancelRequest)
        {
            throw new NotImplementedException();
        }

        private string GetDocumentNumber(IEnumerable<DocumentItemModel> documents)
        {
            var invoice = documents.FirstOrDefault(d => d.DocType == 13)?.DocEntry;
            var payment = documents.FirstOrDefault(d => d.DocType == 24)?.DocEntry;
            var other = documents.Where(d => d.DocType != 24 && d.DocType != 13).Select(d => d.DocEntry).ToList();

            if (payment != null)
            {
                other.Insert(0, payment.Value);
            }

            if (invoice != null)
            {
                other.Insert(0, invoice.Value);
            }

            return string.Join("/", other.Select(d => d.ToString()));
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
