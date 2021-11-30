using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Helpers
{
   public static class RestCall
    {
        public static bool Request(string url, Method method, string json )
        {
            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(method);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", string.Format("Bearer {0}", Common.Helpers.Authorization.GetAuthorizationCode()));
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
