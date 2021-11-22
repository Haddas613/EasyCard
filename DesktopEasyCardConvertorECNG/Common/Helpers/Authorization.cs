using Common.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Helpers
{
   public static class Authorization
    {
        public static string GetAuthorizationCode()
        {
            var client = new RestClient("https://ecng-identity.azurewebsites.net/connect/token");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("client_id", "management_api");
            request.AddParameter("client_secret", "yuwsCT8cbJVgw2W6");
            request.AddParameter("grant_type", "client_credentials");
            IRestResponse response = client.Execute(request);
            AuthorizationCodeResponse authorizationCode = JsonConvert.DeserializeObject<AuthorizationCodeResponse>(response.Content);
            return authorizationCode.access_token;
        }

    }
}
