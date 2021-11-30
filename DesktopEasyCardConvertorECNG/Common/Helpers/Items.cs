using Common.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Helpers
{
    public static class Items
    {
        public static bool AddItem(string itemName,string billingDesktopRefNumber, Models.Enums.CurrencyEnum currency,string externalReference , decimal price, bool active )
        {
            ItemRequest item = new ItemRequest() {  Active = active, BillingDesktopRefNumber = billingDesktopRefNumber, Currency = currency, ExternalReference = externalReference, ItemName = itemName, Price = price };
            var client = new RestClient("https://ecng-profile.azurewebsites.net/api/items/api/items");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", string.Format("Bearer {0}", Common.Helpers.Authorization.GetAuthorizationCode()));
            string jsonAddItem = JsonConvert.SerializeObject(item);
            request.AddParameter("application/json", jsonAddItem/*"{\r\n  \"itemName\": \"Item # 1\",\r\n  \"price\": 1.25,\r\n  \"currency\": \"ILS\"\r\n}"*/, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return response.StatusCode == System.Net.HttpStatusCode.OK;
           // Console.WriteLine(response.Content);
        }
    }
}
