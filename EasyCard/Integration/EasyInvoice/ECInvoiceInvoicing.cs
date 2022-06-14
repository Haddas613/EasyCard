using EasyInvoice.Converters;
using EasyInvoice.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Api.Models;
using Shared.Helpers;
using Shared.Integration;
using Shared.Integration.Exceptions;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
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
            var json = ECInvoiceConverter.GetInvoiceCreateDocumentRequest(documentCreationRequest, terminal);
            json.KeyStorePassword = terminal.KeyStorePassword;

            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            NameValueCollection headers = await
                GetAuthorizedHeaders(terminal.UserName, terminal.Password, integrationMessageId, documentCreationRequest.CorrelationId, documentCreationRequest.InvoiceID);

            ECInvoiceDocumentResponse svcRes = null;
            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;

            try
            {
                string langInvoice = !string.IsNullOrWhiteSpace(documentCreationRequest.InvoiceDetails.InvoiceLanguage) ? documentCreationRequest.InvoiceDetails.InvoiceLanguage : (terminal.Lang.HasValue ? terminal.Lang.Value.ToString().ToLower() : "he");
                headers.Add("Accept-language", langInvoice);

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
                IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, documentCreationRequest.InvoiceID, integrationMessageId, documentCreationRequest.CorrelationId)
                {
                    Request = requestStr,
                    Response = responseStr,
                    ResponseStatus = responseStatusStr,
                    Address = requestUrl,
                    Method = "POST"
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

        public async Task<OperationResponse> CreateCustomer(ECCreateCustomerRequest request, string correlationId)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            var headers = await GetAuthorizedHeaders(configuration.AdminUserName, configuration.AdminPassword, integrationMessageId, correlationId, request.Email);

            try
            {
                headers.Add("Accept-language", "he"); // TODO: get language from options

                /*
                 * Response body: empty
                 * Possible errors:
                    * HTTP 409 (conflict) - user already exists
                 */
                var result = await this.apiClient.Post<object>(this.configuration.BaseUrl, "/api/v1/admin/user", request, () => Task.FromResult(headers));

                return new OperationResponse
                {
                    Status = Shared.Api.Models.Enums.StatusEnum.Success,
                    Message = "User created"
                };
            }
            catch (Exception ex)
            {
                if (ex is WebApiClientErrorException wacEx)
                {
                    if (wacEx.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        return new OperationResponse
                        {
                            Status = Shared.Api.Models.Enums.StatusEnum.Error,
                            Message = "User Already Exists"
                        };
                    }
                }

                this.logger.LogError(ex, $"EasyInvoice CreateCustomer request failed. {ex.Message} ({integrationMessageId}). CorrelationId: {correlationId}");

                throw new IntegrationException("EasyInvoice CreateCustomer request failed", integrationMessageId);
            }
        }

        public async Task<OperationResponse> UpdateCustomer(UpdateUserDetailsRequest request, string correlationId)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            var headers = await GetAuthorizedHeaders(configuration.AdminUserName, configuration.AdminPassword, integrationMessageId, correlationId, request.email);

            try
            {
                headers.Add("Accept-language", "he"); // TODO: get language from options

                UpdateUserDetailsRequest req = ECInvoiceConverter.GetUpdateUserDataRequest(request);
                /*
                 * Response body: empty
                 * Possible errors:
                    * HTTP 409 (conflict) - user already exists
                 */
                var result = await this.apiClient.Patch<object>(this.configuration.BaseUrl, "/api/v1/user", req, () => Task.FromResult(headers));

                return new OperationResponse
                {
                    Status = Shared.Api.Models.Enums.StatusEnum.Success,
                    Message = "User updated"
                };
            }
            catch (Exception ex)
            {
                if (ex is WebApiClientErrorException wacEx)
                {
                    if (wacEx.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        return new OperationResponse
                        {
                            Status = Shared.Api.Models.Enums.StatusEnum.Error,
                            Message = "User Already Exists"
                        };
                    }
                }

                this.logger.LogError(ex, $"EasyInvoice UpdateCustomer request failed. {ex.Message} ({integrationMessageId}). CorrelationId: {correlationId}");

                throw new IntegrationException("EasyInvoice UpdateCustomer request failed", integrationMessageId);
            }
        }

        public async Task<OperationResponse> GenerateCertificate(EasyInvoiceTerminalSettings terminal, string correlationId)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            NameValueCollection headers = await GetAuthorizedHeaders(terminal.UserName, terminal.Password, integrationMessageId, correlationId, terminal.UserName);

            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;

            try
            {
                headers.Add("Accept-language", "he"); // TODO: get language from options

                var json = new ECInvoiceGenerateCertificateRequest
                {
                    FirstLastName = Guid.NewGuid().ToString() // TODO
                };

                var svcRes = await this.apiClient.Post<ECInvoiceGenerateCertificateResponse>(this.configuration.BaseUrl, "/api/v1/user/generate-certificate", json, () => Task.FromResult(headers),
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

                return new OperationResponse { Status = Shared.Api.Models.Enums.StatusEnum.Success, EntityReference = svcRes.KeyStorePassword };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"EasyInvoice integration request failed. {ex.Message} ({integrationMessageId}). CorrelationId: {correlationId}");

                throw new IntegrationException("EasyInvoice integration request failed", integrationMessageId);
            }
            finally
            {
                IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, terminal.UserName, integrationMessageId, correlationId)
                {
                    Request = requestStr,
                    Response = responseStr,
                    ResponseStatus = responseStatusStr,
                    Address = requestUrl
                };

                await storageService.Save(integrationMessage);
            }
        }

        public async Task<OperationResponse> SetDocumentNumber(ECInvoiceSetDocumentNumberRequest request, string correlationId)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            var headers = await GetAuthorizedHeaders(request.Terminal.UserName, request.Terminal.Password, integrationMessageId, correlationId, request.Email);

            try
            {
                // headers.Add("Accept-language", "he"); // TODO: get language from options

                var json = new SetDocNextNumberModel
                {
                    DocumentType = request.DocType.ToString(),
                    NextDocumentNumber = request.CurrentNum
                };

                var result = await this.apiClient.Post<Object>(this.configuration.BaseUrl, "/api/v1/user/document-settings", json, () => Task.FromResult(headers));

                return new OperationResponse
                {
                    Status = Shared.Api.Models.Enums.StatusEnum.Success,
                    Message = "Document Number Changed"
                };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"EasyInvoice Change Document Number request failed. {ex.Message} ({integrationMessageId}). CorrelationId: {correlationId}");

                throw new IntegrationException("EasyInvoice Change Document Number request failed", integrationMessageId);
            }
        }

        public async Task<DocumentNextNumberModel> GetDocumentNumber(ECInvoiceGetDocumentNumberRequest request, string correlationId)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            var headers = await GetAuthorizedHeaders(request.Terminal.UserName, request.Terminal.Password, integrationMessageId, correlationId, "");

            try
            {
                // headers.Add("Accept-language", "he"); // TODO: get language from options

                var json = new GetDocNumberModel
                {
                    DocumentType = request.DocType.ToString(),
                };

                var result = await this.apiClient.Get<DocumentNextNumberModel>(this.configuration.BaseUrl, "/api/v1/user/document-settings", json, () => Task.FromResult(headers));
                return result;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"EasyInvoice Get Document Number request failed. {ex.Message} ({integrationMessageId}). CorrelationId: {correlationId}");

                throw new IntegrationException("EasyInvoice Get Document Number request failed", integrationMessageId);
            }
        }

        public async Task<IEnumerable<ECInvoiceGetReportItem>> GetReport(ECInvoiceGetDocumentReportRequest request, string correlationId)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            var headers = await GetAuthorizedHeaders(request.Terminal.UserName, request.Terminal.Password, integrationMessageId, correlationId, "");

            try
            {
                // headers.Add("Accept-language", "he"); // TODO: get language from options

                var json = new GetDocumentReportModel
                {
                    EndDate = request.EndDate,
                    StartDate = request.StartDate,
                    IncludeCancelled = request.IncludeCancelled,
                    OnlyCancelled = request.OnlyCancelled,
                };

                var result = await this.apiClient.Get<ECInvoiceGetReportItem[]>(this.configuration.BaseUrl, "/api/v1/report", json, () => Task.FromResult(headers));
                return result;
                //    return new OperationResponse
                //    {
                //        //EntityID = result
                //        Status = Shared.Api.Models.Enums.StatusEnum.Success,
                //        Message = "Get Document Number",
                //         
                //    };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"EasyInvoice Get Report request failed. {ex.Message} ({integrationMessageId}). CorrelationId: {correlationId}");

                throw new IntegrationException("EasyInvoice Get Report request failed", integrationMessageId);
            }
        }

        public async Task<Object> GetTaxReport(ECInvoiceGetDocumentTaxReportRequest request, string correlationId)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            var headers = await GetAuthorizedHeaders(request.Terminal.UserName, request.Terminal.Password, integrationMessageId, correlationId, "");

            try
            {
                // headers.Add("Accept-language", "he"); // TODO: get language from options

                var json = new GetDocumentTaxReportModel
                {
                    endDate = request.EndDate,
                    startDate = request.StartDate,
                };

                var result = await this.apiClient.GetObj<Object>(this.configuration.BaseUrl, "/api/v1/tax-report", json, () => Task.FromResult(headers));
                return result;
                //    return new OperationResponse
                //    {
                //        //EntityID = result
                //        Status = Shared.Api.Models.Enums.StatusEnum.Success,
                //        Message = "Get Document Number",
                //         
                //    };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"EasyInvoice Get Tax Report request failed. {ex.Message} ({integrationMessageId}). CorrelationId: {correlationId}");

                throw new IntegrationException("EasyInvoice Get Tax Report request failed", integrationMessageId);
            }
        }

        public async Task<Object> GetHashReport(ECInvoiceGetDocumentTaxReportRequest request, string correlationId)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            var headers = await GetAuthorizedHeaders(request.Terminal.UserName, request.Terminal.Password, integrationMessageId, correlationId, "");

            try
            {
                // headers.Add("Accept-language", "he"); // TODO: get language from options

                var json = new GetDocumentTaxReportModel
                {
                    endDate = request.EndDate,
                    startDate = request.StartDate,
                };

                var result = await this.apiClient.GetObj<Object>(this.configuration.BaseUrl, "/api/v1/hash-report", json, () => Task.FromResult(headers));
                return result;
                //    return new OperationResponse
                //    {
                //        //EntityID = result
                //        Status = Shared.Api.Models.Enums.StatusEnum.Success,
                //        Message = "Get Document Number",
                //         
                //    };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"EasyInvoice Get Hash Report request failed. {ex.Message} ({integrationMessageId}). CorrelationId: {correlationId}");

                throw new IntegrationException("EasyInvoice Get Hash Report request failed", integrationMessageId);
            }
        }

        public async Task<OperationResponse> GetDocumentTypes(ECInvoiceGetDocumentNumberRequest request, string correlationId)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            var headers = await GetAuthorizedHeaders(request.Terminal.UserName, request.Terminal.Password, integrationMessageId, correlationId, "");

            try
            {
                var result = await this.apiClient.Get<DocumentTypeModel>(this.configuration.BaseUrl, "/api/v1/document-types", null, () => Task.FromResult(headers));
                return new OperationResponse
                {
                    Status = Shared.Api.Models.Enums.StatusEnum.Success,
                    Message = "Get Document Types",
                };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"EasyInvoice Get Document Types request failed. {ex.Message} ({integrationMessageId}). CorrelationId: {correlationId}");

                throw new IntegrationException("EasyInvoice Get Document Types request failed", integrationMessageId);
            }
        }

        public async Task<OperationResponse> UploadUserLogo(EasyInvoiceTerminalSettings settings, MemoryStream stream, string fileName, string correlationId)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            var headers = await GetAuthorizedHeaders(settings.UserName, settings.Password, integrationMessageId, correlationId, settings.UserName);

            var extension = Path.GetExtension(fileName);

            /*
            * * Accepted file formats: `.png`, `.jpg`, `.jpeg`
            */
            var allowedImageTypes = new HashSet<string>(3) { ".jpeg", ".jpg", ".png" };
            if (!allowedImageTypes.Contains(extension))
            {
                return new OperationResponse
                {
                    Status = Shared.Api.Models.Enums.StatusEnum.Error,
                    Message = $"{extension} is not supported"
                };
            }

            try
            {
                headers.Add("Accept-language", "he"); // TODO: get language from options

                /*
                 * Example response body:
                    #!json
                    {
                        "logoUrl": "https://s3-eu-west-1.amazonaws.com/invoicesystem-logos-stage/1/customer_logo"
                    }
                 */

                var result = await this.apiClient.PostFile(this.configuration.BaseUrl, "/api/v1/user/logo", stream, fileName, "file", () => Task.FromResult(headers));

                return new OperationResponse
                {
                    Status = Shared.Api.Models.Enums.StatusEnum.Success,
                    Message = "Logo uploaded"
                };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"EasyInvoice UploadUserLogo request failed. {ex.Message} ({integrationMessageId}). CorrelationId: {correlationId}");

                throw new IntegrationException("EasyInvoice UploadUserLogo request failed", integrationMessageId);
            }
        }

        public Task<IEnumerable<string>> GetDownloadUrls(JObject externalSystemData, object invoiceingSettings)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IntegrationMessage>> GetStorageLogs(string entityID)
        {
            return storageService.GetAll(entityID);
        }

        public Task<CreateConsumerResponse> CreateConsumerOrGetExisting(CreateConsumerRequest consumerRequest)
        {
            throw new NotImplementedException();
        }

        public bool CanCreateConsumer()
        {
            return false;
        }

        public bool CanCancelDocument()
        {
            return true;
        }

        public async Task<InvoicingCancelDocumentResponse> CancelDocument(InvoicingCancelDocumentRequest documentCancelRequest)
        {
            var terminal = documentCancelRequest.InvoiceingSettings as EasyInvoiceTerminalSettings;

            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            NameValueCollection headers = await
                GetAuthorizedHeaders(terminal.UserName, terminal.Password, integrationMessageId, documentCancelRequest.CorrelationId, documentCancelRequest.InvoiceID);

            ECInvoiceDocumentResponse svcRes = null;
            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;

            try
            {
                var invoiceType = ECInvoiceConverter.GetECInvoiceDocumentType(documentCancelRequest.InvoiceDetails.InvoiceType, documentCancelRequest.InvoiceDetails.Donation);

                svcRes = await this.apiClient.Delete<ECInvoiceDocumentResponse>(this.configuration.BaseUrl, $"/api/v1/docs/{invoiceType}/{documentCancelRequest.InvoiceNumber}", () => Task.FromResult(headers),
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
                this.logger.LogError(ex, $"EasyInvoice integration request failed. {ex.Message} ({integrationMessageId}). CorrelationId: {documentCancelRequest.CorrelationId}");

                throw new IntegrationException("EasyInvoice integration request failed", integrationMessageId);
            }
            finally
            {
                IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, documentCancelRequest.InvoiceID, integrationMessageId, documentCancelRequest.CorrelationId)
                {
                    Request = requestStr,
                    Response = responseStr,
                    ResponseStatus = responseStatusStr,
                    Address = requestUrl,
                    Method = "DELETE"
                };

                await storageService.Save(integrationMessage);
            }

            var response = new InvoicingCancelDocumentResponse
            {
                DocumentNumber = svcRes.DocumentNumber?.ToString()
            };

            if (svcRes.Errors?.Count > 0 || !string.IsNullOrWhiteSpace(svcRes.Error) || string.IsNullOrWhiteSpace(svcRes.DocumentUrl) || svcRes.DocumentNumber.GetValueOrDefault() <= 0)
            {
                response.Success = false;
                response.ErrorMessage = svcRes.Message ?? svcRes.Error;
            }

            return response;
        }

        private async Task<NameValueCollection> GetAuthorizedHeaders(string username, string password, string integrationMessageId, string correlationId, string entityId)
        {
            string requestUrl = null;
            string requestStr = null;
            string responseStr = null;
            string responseStatusStr = null;

            try
            {
                var loginRequest = new { email = username, password = password };

                var loginres = await apiClient.PostRawWithHeaders(this.configuration.BaseUrl, "/api/v1/login", JsonConvert.SerializeObject(loginRequest), "application/json", null,
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

                var authToken = loginres.ResponseHeaders.AllKeys.Any(k => k == ResponseTokenHeader) ? loginres.ResponseHeaders[ResponseTokenHeader] : null;

                NameValueCollection headers = new NameValueCollection();
                headers.Add(ResponseTokenHeader, authToken);

                return headers;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"EasyInvoice integration request failed: failed to get token: {ex.Message} ({integrationMessageId}). CorrelationId: {correlationId}");

                IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, entityId, integrationMessageId, correlationId)
                {
                    Request = requestStr,
                    Response = responseStr,
                    ResponseStatus = responseStatusStr,
                    Address = requestUrl
                };

                await storageService.Save(integrationMessage);

                throw new IntegrationException("EasyInvoice integration request failed: failed to get token", integrationMessageId);
            }
        }
    }
}
