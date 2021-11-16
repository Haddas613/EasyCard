﻿using EasyInvoice.Converters;
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

            var json = ECInvoiceConverter.GetInvoiceCreateDocumentRequest(documentCreationRequest);
            json.KeyStorePassword = terminal.KeyStorePassword;

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
                IntegrationMessage integrationMessage = new IntegrationMessage(DateTime.UtcNow, documentCreationRequest.InvoiceID, integrationMessageId, documentCreationRequest.CorrelationId)
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

        public async Task<OperationResponse> CreateCustomer(ECCreateCustomerRequest request, string correlationId)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            var headers = GetAuthorizedHeaders(configuration.AdminUserName, configuration.AdminPassword, integrationMessageId, correlationId);

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

        public async Task<OperationResponse> GenerateCertificate(EasyInvoiceTerminalSettings terminal, string correlationId)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            NameValueCollection headers = GetAuthorizedHeaders(terminal.UserName, terminal.Password, integrationMessageId, correlationId);

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

        public async Task<OperationResponse> UploadUserLogo(EasyInvoiceTerminalSettings settings, MemoryStream stream, string fileName, string correlationId)
        {
            var integrationMessageId = Guid.NewGuid().GetSortableStr(DateTime.UtcNow);

            var headers = GetAuthorizedHeaders(settings.UserName, settings.Password, integrationMessageId, correlationId);

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

        private NameValueCollection GetAuthorizedHeaders(string username, string password, string integrationMessageId, string correlationId)
        {
            try
            {
                var loginRequest = new { email = username, password = password };

                var loginres = apiClient.PostRawWithHeaders(this.configuration.BaseUrl, "/api/v1/login", JsonConvert.SerializeObject(loginRequest), "application/json").Result;

                var authToken = loginres.ResponseHeaders.AllKeys.Any(k => k == ResponseTokenHeader) ? loginres.ResponseHeaders[ResponseTokenHeader] : null;

                NameValueCollection headers = new NameValueCollection();
                headers.Add(ResponseTokenHeader, authToken);

                return headers;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"EasyInvoice integration request failed: failed to get token: {ex.Message} ({integrationMessageId}). CorrelationId: {correlationId}");

                throw new IntegrationException("EasyInvoice integration request failed: failed to get token", integrationMessageId);
            }
        }
    }
}
