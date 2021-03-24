﻿using Newtonsoft.Json;
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
        public HttpClient HttpClient { get; private set; }

        public WebApiClient()
        {
            var hadler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip };
            hadler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }; // TODO: this can be used only during dev cycle

            HttpClient = new HttpClient(hadler);

            HttpClient.DefaultRequestHeaders.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("gzip"));
            HttpClient.DefaultRequestHeaders.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("defalte"));
        }

        public void Dispose()
        {
            if (HttpClient != null)
            {
                HttpClient.Dispose();
                HttpClient = null;
            }
        }

        public async Task<T> Get<T>(string enpoint, string actionPath, object querystr = null, Func<Task<NameValueCollection>> getHeaders = null)
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

            HttpResponseMessage response = await HttpClient.SendAsync(request);
            var res = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(res);
            }
            else
            {
                if ((int)response.StatusCode >= 500)
                {
                    throw new WebApiServerErrorException($"Failed GET from {url}: {response.StatusCode}", response.StatusCode, res);
                }
                else if ((int)response.StatusCode >= 400)
                {
                    throw new WebApiClientErrorException($"Failed GET from {url}: {response.StatusCode}", response.StatusCode, res);
                }
                else
                {
                    throw new ApplicationException($"Failed GET from {url}: {response.StatusCode}");
                }
            }
        }

        public async Task<T> Post<T>(string enpoint, string actionPath, object payload, Func<Task<NameValueCollection>> getHeaders = null,
            ProcessRequest onRequest = null, ProcessResponse onResponse = null
            )
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

            onRequest?.Invoke(url, json);

            HttpResponseMessage response = await HttpClient.SendAsync(request);

            var res = await response.Content.ReadAsStringAsync();

            onResponse?.Invoke(res, response.StatusCode, response.Headers);

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(res);
            }
            else
            {
                if ((int)response.StatusCode >= 500)
                {
                    throw new WebApiServerErrorException($"Failed POST from {url}: {response.StatusCode}", response.StatusCode, res);
                }
                else if ((int)response.StatusCode >= 400)
                {
                    throw new WebApiClientErrorException($"Failed POST from {url}: {response.StatusCode}", response.StatusCode, res);
                }
                else
                {
                    throw new ApplicationException($"Failed POST from {url}: {response.StatusCode}");
                }
            }
        }

        public async Task<T> Put<T>(string enpoint, string actionPath, object payload, Func<Task<NameValueCollection>> getHeaders = null,
            ProcessRequest onRequest = null, ProcessResponse onResponse = null
            )
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

            onRequest?.Invoke(url, json);

            HttpResponseMessage response = await HttpClient.SendAsync(request);

            var res = await response.Content.ReadAsStringAsync();

            onResponse?.Invoke(res, response.StatusCode, response.Headers);

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(res);
            }
            else
            {
                if ((int)response.StatusCode >= 500)
                {
                    throw new WebApiServerErrorException($"Failed PUT from {url}: {response.StatusCode}", response.StatusCode, res);
                }
                else if ((int)response.StatusCode >= 400)
                {
                    throw new WebApiClientErrorException($"Failed PUT from {url}: {response.StatusCode}", response.StatusCode, res);
                }
                else
                {
                    throw new ApplicationException($"Failed PUT from {url}: {response.StatusCode}");
                }
            }
        }

        public async Task<T> PostXml<T>(string enpoint, string actionPath, object payload, Func<Task<NameValueCollection>> getHeaders = null,
            ProcessRequest onRequest = null, ProcessResponse onResponse = null
            )
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

            onRequest?.Invoke(url, xml);

            HttpResponseMessage response = await HttpClient.SendAsync(request);

            var res = await response.Content.ReadAsStringAsync();

            onResponse?.Invoke(res, response.StatusCode, response.Headers);

            if (response.IsSuccessStatusCode)
            {
                return res.FromXml<T>();
            }
            else
            {
                if ((int)response.StatusCode >= 500)
                {
                    throw new WebApiServerErrorException($"Failed POST from {url}: {response.StatusCode}", response.StatusCode, res);
                }
                else if ((int)response.StatusCode >= 400)
                {
                    throw new WebApiClientErrorException($"Failed POST from {url}: {response.StatusCode}", response.StatusCode, res);
                }
                else
                {
                    throw new ApplicationException($"Failed POST from {url}: {response.StatusCode}");
                }
            }
        }

        public async Task<string> PostRaw(string enpoint, string actionPath, string payload, string contentType, Func<Task<NameValueCollection>> getHeaders = null,
            ProcessRequest onRequest = null, ProcessResponse onResponse = null
            )
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

            onRequest?.Invoke(url, payload);

            HttpResponseMessage response = await HttpClient.SendAsync(request);

            var res = await response.Content.ReadAsStringAsync();

            onResponse?.Invoke(res, response.StatusCode, response.Headers);

            if (response.IsSuccessStatusCode)
            {
                return res;
            }
            else
            {
                if ((int)response.StatusCode >= 500)
                {
                    throw new WebApiServerErrorException($"Failed POST from {url}: {response.StatusCode}", response.StatusCode, res);
                }
                else if ((int)response.StatusCode >= 400)
                {
                    throw new WebApiClientErrorException($"Failed POST from {url}: {response.StatusCode}", response.StatusCode, res);
                }
                else
                {
                    throw new ApplicationException($"Failed POST from {url}: {response.StatusCode}");
                }
            }
        }

        public async Task<string> PostRawForm(string enpoint, string actionPath, IDictionary<string, string> payload, Func<Task<NameValueCollection>> getHeaders = null)
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

            HttpResponseMessage response = await HttpClient.SendAsync(request);

            var res = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return res;
            }
            else
            {
                if ((int)response.StatusCode >= 500)
                {
                    throw new WebApiServerErrorException($"Failed POST from {url}: {response.StatusCode}", response.StatusCode, res);
                }
                else if ((int)response.StatusCode >= 400)
                {
                    throw new WebApiClientErrorException($"Failed POST from {url}: {response.StatusCode}", response.StatusCode, res);
                }
                else
                {
                    throw new ApplicationException($"Failed POST from {url}: {response.StatusCode}");
                }
            }
        }

        public async Task<T> Delete<T>(string enpoint, string actionPath, Func<Task<NameValueCollection>> getHeaders = null,
            ProcessRequest onRequest = null, ProcessResponse onResponse = null
            )
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

            onRequest?.Invoke(url, string.Empty);

            var response = await HttpClient.SendAsync(request);

            onResponse?.Invoke(string.Empty, response.StatusCode, response.Headers);

            var data = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(data);
            }
            else
            {
                if ((int)response.StatusCode >= 500)
                {
                    throw new WebApiServerErrorException($"Failed DELETE from {url}: {response.StatusCode}", response.StatusCode, data);
                }
                else if ((int)response.StatusCode >= 400)
                {
                    throw new WebApiClientErrorException($"Failed DELETE from {url}: {response.StatusCode}", response.StatusCode, data);
                }
                else
                {
                    throw new ApplicationException($"Failed DELETE from {url}: {response.StatusCode}");
                }
            }
        }

        public async Task<RawRequestResult> PostRawWithHeaders(string enpoint, string actionPath, string payload, string contentType,
            Func<Task<NameValueCollection>> getHeaders = null, ProcessRequest onRequest = null, ProcessResponse onResponse = null
            )
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

            onRequest?.Invoke(url, payload);

            HttpResponseMessage response = await HttpClient.SendAsync(request);

            var operationResponse = new RawRequestResult();
            operationResponse.StatusCode = response.StatusCode;

            foreach (var header in response.Headers)
            {
                operationResponse.ResponseHeaders.Add(header.Key, header.Value?.FirstOrDefault());
            }

            var res = await response.Content.ReadAsStringAsync();

            onResponse?.Invoke(res, response.StatusCode, response.Headers);

            if (response.IsSuccessStatusCode)
            {
                return operationResponse;
            }
            else
            {
                throw new ApplicationException($"Failed POST to {url}: {response.StatusCode}");
            }
        }
    }
}
