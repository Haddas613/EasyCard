﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Shared.Helpers
{
    public interface IWebApiClient
    {
        Task<T> Get<T>(string enpoint, string actionPath, object querystr = null, Func<Task<NameValueCollection>> getHeaders = null);

        Task<T> Post<T>(string enpoint, string actionPath, object payload, Func<Task<NameValueCollection>> getHeaders = null,
            ProcessRequest onRequest = null, ProcessResponse onResponse = null
            );

        Task<T> Put<T>(string enpoint, string actionPath, object payload, Func<Task<NameValueCollection>> getHeaders = null,
            ProcessRequest onRequest = null, ProcessResponse onResponse = null
            );

        Task<T> Delete<T>(string enpoint, string actionPath, Func<Task<NameValueCollection>> getHeaders = null,
            ProcessRequest onRequest = null, ProcessResponse onResponse = null
            );

        Task<T> PostXml<T>(string enpoint, string actionPath, object payload, Func<Task<NameValueCollection>> getHeaders = null,
            ProcessRequest onRequest = null, ProcessResponse onResponse = null
            );

        Task<string> PostRaw(string enpoint, string actionPath, string payload, string contentType, Func<Task<NameValueCollection>> getHeaders = null,
            ProcessRequest onRequest = null, ProcessResponse onResponse = null
            );

        Task<string> PostRawForm(string enpoint, string actionPath, IDictionary<string, string> payload, Func<Task<NameValueCollection>> getHeaders = null);

        Task<RawRequestResult> PostRawWithHeaders(string enpoint, string actionPath, string payload, string contentType, Func<Task<NameValueCollection>> getHeaders = null,
            ProcessRequest onRequest = null, ProcessResponse onResponse = null
            );

    }

    public delegate void ProcessRequest(string url, string request);

    public delegate void ProcessResponse(string response, HttpStatusCode responseStatus, HttpResponseHeaders responseHeaders);
}
