using Shared.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace Shared.Helpers
{
    public class WebApiClient : IWebApiClient, IDisposable
    {
        private HttpClient httpClient;

        public WebApiClient()
        {
            var hadler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip };
            hadler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }; // TODO: this can be used only during dev cycle

            httpClient = new HttpClient(hadler);

            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("gzip"));
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("defalte"));
        }

        public void Dispose()
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
                httpClient = null;
            }
        }


        public async Task<T> Get<T>(string enpoint, string actionPath, object querystr = null, Func<Task<NameValueCollection>> getHeaders = null) where T : class
        {
            var url = UrlHelper.BuildUrl(enpoint, actionPath, querystr);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            if (getHeaders != null)
            {
                var headers = await getHeaders();
                foreach (var header in headers.AllKeys)
                {
                    request.Headers.Add(header, headers.GetValues(header).FirstOrDefault());
                }
            }

            HttpResponseMessage response = await httpClient.SendAsync(request);
            var res = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(res);
            }
            else
            {
                throw new ApplicationException($"Failed GET from {url}: {response.StatusCode}");
            }
        }

        public async Task<T> Post<T>(string enpoint, string actionPath, object payload, Func<Task<NameValueCollection>> getHeaders = null, IntegrationMessage integrationMessage = null) where T : class
        {
            var url = UrlHelper.BuildUrl(enpoint, actionPath);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

            if (getHeaders != null)
            {
                var headers = await getHeaders();
                foreach (var header in headers.AllKeys)
                {
                    request.Headers.Add(header, headers.GetValues(header).FirstOrDefault());
                }
            }

            var json = JsonConvert.SerializeObject(payload);

            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            if (integrationMessage != null)
            {
                integrationMessage.Address = url;
                integrationMessage.Request = json;
            }

            HttpResponseMessage response = await httpClient.SendAsync(request);

            var res = await response.Content.ReadAsStringAsync();

            if (integrationMessage != null)
            {
                integrationMessage.Response = res;
                integrationMessage.ResponseStatus = response.StatusCode.ToString();
            }

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(res);
            }
            else
            {
                throw new ApplicationException($"Failed POST to {url}: {response.StatusCode}");
            }
        }

        public async Task<T> Put<T>(string enpoint, string actionPath, object payload, Func<Task<NameValueCollection>> getHeaders = null) where T : class
        {
            var url = UrlHelper.BuildUrl(enpoint, actionPath);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url);

            if (getHeaders != null)
            {
                var headers = await getHeaders();
                foreach (var header in headers.AllKeys)
                {
                    request.Headers.Add(header, headers.GetValues(header).FirstOrDefault());
                }
            }

            var json = JsonConvert.SerializeObject(payload);

            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.SendAsync(request);

            var res = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(res);
            }
            else
            {
                throw new ApplicationException($"Failed PUT to {url}: {response.StatusCode}");
            }
        }

        public async Task<T> PostXml<T>(string enpoint, string actionPath, object payload, Func<Task<NameValueCollection>> getHeaders = null, IntegrationMessage integrationMessage = null) where T : class
        {
            var url = UrlHelper.BuildUrl(enpoint, actionPath);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

            if (getHeaders != null)
            {
                var headers = await getHeaders();
                foreach (var header in headers.AllKeys)
                {
                    request.Headers.Add(header, headers.GetValues(header).FirstOrDefault());
                }
            }

            var xml = payload.ToXml();

            request.Content = new StringContent(xml, Encoding.UTF8, "text/xml");

            if (integrationMessage != null)
            {
                integrationMessage.Address = url;
                integrationMessage.Request = xml;
            }

            HttpResponseMessage response = await httpClient.SendAsync(request);

            var res = await response.Content.ReadAsStringAsync();

            if (integrationMessage != null)
            {
                integrationMessage.Response = res;
                integrationMessage.ResponseStatus = response.StatusCode.ToString();
            }

            if (response.IsSuccessStatusCode)
            {
                return res.FromXml<T>();
            }
            else
            {
                throw new ApplicationException($"Failed POST to {url}: {response.StatusCode}");
            }
        }

        public async Task<string> PostRaw(string enpoint, string actionPath, string payload, string contentType, Func<Task<NameValueCollection>> getHeaders = null)
        {
            var url = UrlHelper.BuildUrl(enpoint, actionPath);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

            if (getHeaders != null)
            {
                var headers = await getHeaders();
                foreach (var header in headers.AllKeys)
                {
                    request.Headers.Add(header, headers.GetValues(header).FirstOrDefault());
                }
            }

            request.Content = new StringContent(payload, Encoding.UTF8, contentType ?? "text/xml");

            HttpResponseMessage response = await httpClient.SendAsync(request);

            var res = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return res;
            }
            else
            {
                throw new ApplicationException($"Failed POST to {url}: {response.StatusCode}");
            }
        }

        public async Task<RawRequestResult> PostRawWithHeaders(string enpoint, string actionPath, string payload, string contentType, Func<Task<NameValueCollection>> getHeaders = null)
        {
            var url = UrlHelper.BuildUrl(enpoint, actionPath);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

            if (getHeaders != null)
            {
                var headers = await getHeaders();
                foreach (var header in headers.AllKeys)
                {
                    request.Headers.Add(header, headers.GetValues(header).FirstOrDefault());
                }
            }

            request.Content = new StringContent(payload, Encoding.UTF8, contentType ?? "text/xml");

            HttpResponseMessage response = await httpClient.SendAsync(request);

            var operationResponse = new RawRequestResult();
            operationResponse.StatusCode = response.StatusCode;
            

            foreach (var header in response.Headers)
            {
                operationResponse.ResponseHeaders.Add(header.Key, header.Value?.FirstOrDefault());
            }

            var res = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return operationResponse;
            }
            else
            {
                throw new ApplicationException($"Failed POST to {url}: {response.StatusCode}");
            }
        }

        public async Task<string> PostRawForm(string enpoint, string actionPath, IDictionary<string,string> payload, Func<Task<NameValueCollection>> getHeaders = null, IntegrationMessage integrationMessage = null)
        {
            var url = UrlHelper.BuildUrl(enpoint, actionPath);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

            if (getHeaders != null)
            {
                var headers = await getHeaders();
                foreach (var header in headers.AllKeys)
                {
                    request.Headers.Add(header, headers.GetValues(header).FirstOrDefault());
                }
            }



            request.Content = new FormUrlEncodedContent(payload);

            if (integrationMessage != null)
            {
                integrationMessage.Address = url;
            }

            HttpResponseMessage response = await httpClient.SendAsync(request);

            var res = await response.Content.ReadAsStringAsync();

            if (integrationMessage != null)
            {
                integrationMessage.Response = res;
                integrationMessage.ResponseStatus = response.StatusCode.ToString();
            }

            if (response.IsSuccessStatusCode)
            {
                return res;
            }
            else
            {
                throw new ApplicationException($"Failed POST to {url}: {response.StatusCode}");
            }
        }

        public async Task<T> Delete<T>(string enpoint, string actionPath, Func<Task<NameValueCollection>> getHeaders = null)
        {
            var url = UrlHelper.BuildUrl(enpoint, actionPath);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);

            if (getHeaders != null)
            {
                var headers = await getHeaders();
                foreach (var header in headers.AllKeys)
                {
                    request.Headers.Add(header, headers.GetValues(header).FirstOrDefault());
                }
            }

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(data);
            }

            throw new ApplicationException($"Failed PUT to {url}: {response.StatusCode}");          
        }
    }
}
