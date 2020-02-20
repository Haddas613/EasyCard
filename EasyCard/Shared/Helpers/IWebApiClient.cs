using Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Shared.Helpers
{
    public interface IWebApiClient
    {
        Task<T> Get<T>(string enpoint, string actionPath, object querystr = null, Func<Task<NameValueCollection>> getHeaders = null) where T : class;

        Task<T> Post<T>(string enpoint, string actionPath, object payload, Func<Task<NameValueCollection>> getHeaders = null, IntegrationMessage integrationMessage = null) where T : class;

        Task<T> PostXml<T>(string enpoint, string actionPath, object payload, Func<Task<NameValueCollection>> getHeaders = null, IntegrationMessage integrationMessage = null) where T : class;

        Task<string> PostRaw(string enpoint, string actionPath, string payload, string contentType, Func<Task<NameValueCollection>> getHeaders = null);

        Task<string> PostRawForm(string enpoint, string actionPath, IDictionary<string, string> payload, Func<Task<NameValueCollection>> getHeaders = null, IntegrationMessage integrationMessage = null);

        Task<RawRequestResult> PostRawWithHeaders(string enpoint, string actionPath, string payload, string contentType, Func<Task<NameValueCollection>> getHeaders = null);

        Task<T> Delete<T>(string enpoint, string actionPath, Func<Task<NameValueCollection>> getHeaders = null);
    }
}
