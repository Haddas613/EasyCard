using Shared.Integration.ExternalSystems;
using System;

namespace EasyInvoice
{
    public class ECInvoiceInvoicing
    {
        public void Process()
        {
            //DocumentResponse svcRes = null;

            //try
            //{
            //    var loginRequest = new { email = this.configuration.UserName, password = this.configuration.Password };

            //    var loginres = apiClient.PostRawWithHeaders(this.configuration.EasyInvoiceBaseUrl, "/api/v1/login", JsonConvert.SerializeObject(loginRequest), "application/json").Result;

            //    var authToken = loginres.ResponseHeaders.AllKeys.Any(k => k == responseTokenHeader) ? loginres.ResponseHeaders[responseTokenHeader] : this.configuration.AuthToken;

            //    NameValueCollection headers = new NameValueCollection();

            //    headers.Add(responseTokenHeader, authToken);
            //    headers.Add("Accept-language", "he");

            //    svcRes = await this.apiClient.Post<DocumentResponse>(this.configuration.EasyInvoiceBaseUrl, "/api/v1/docs", json, () => Task.FromResult(headers), integrationMessage);
            //}
            //catch (Exception ex)
            //{
            //    var correlationId = this.httpContextAccessor?.HttpContext?.TraceIdentifier;

            //    this.logger.LogError(ex, $"Exception {correlationId}: {ex.Message} ({message.MessageId})");

            //    await storageService.Save(integrationMessage);

            //    throw new IntegrationException("EasyInvoice integration request failed", message.MessageId);
            //}

            //await storageService.Save(integrationMessage);

            //if (svcRes == null) throw new IntegrationException("EasyInvoice integration request failed", message.MessageId);

            //var response = new OperationResponse() { Status = StatusEnum.Success, EntityID = svcRes.DocumentNumber, EntityReference = svcRes.DocumentUrl, DocumentUrl = svcRes.DocumentUrl, DocumentCopyUrl = svcRes.DocumentCopyUrl };

            //if (svcRes.Errors?.Count > 0 || !string.IsNullOrWhiteSpace(svcRes.Error) || string.IsNullOrWhiteSpace(svcRes.DocumentUrl) || svcRes.DocumentNumber.GetValueOrDefault() <= 0)
            //{
            //    response.Status = StatusEnum.Error;
            //    response.Message = svcRes.Message ?? svcRes.Error;
            //}

            //return response;
        }
    }
}
