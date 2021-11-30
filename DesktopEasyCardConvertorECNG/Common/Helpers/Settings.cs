using Common.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Helpers
{
   public static  class Settings
    {
       
        public static void CheckSettings(string MerchantID, string TerminalID,out bool MerchantNTerminalExist, out bool BillingAllowed)
        {
            var client = new RestClient(string.Format("https://ecng-merchants.azurewebsites.net/api/terminals/{0}",TerminalID));
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization",string.Format("Bearer {0}",Common.Helpers.Authorization.GetAuthorizationCode()));
            request.AddParameter("application/json", "", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            TerminalResponse merchantDetails = JsonConvert.DeserializeObject<TerminalResponse>(response.Content);
            BillingAllowed = Array.Exists(merchantDetails.enabledFeatures,x => x ==  "Billing");
            MerchantNTerminalExist =  merchantDetails.merchant.merchantID.Equals(MerchantID);
            return;
        }

        public static bool UpdateTerminalSettings(string TerminalID, string InstituteServiceNum, string InstituteName, string InstituteNum, decimal EuroRate, decimal DollarRate, bool VatExempt)
        {
            var client = new RestClient(string.Format("https://ecng-merchants.azurewebsites.net/api/terminals/{0}", TerminalID));
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", string.Format("Bearer {0}", Common.Helpers.Authorization.GetAuthorizationCode()));
            request.AddParameter("application/json", "", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            UpdateTerminalRequest terminalDetails = JsonConvert.DeserializeObject<UpdateTerminalRequest>(response.Content);

            terminalDetails.BankDetails.InstituteServiceNum = InstituteServiceNum;
            terminalDetails.BankDetails.InstituteName = InstituteName;
            terminalDetails.BankDetails.InstituteNum = InstituteNum;

            terminalDetails.Settings.DollarRate = DollarRate;
            terminalDetails.Settings.EuroRate = EuroRate;

            terminalDetails.BillingSettings.CreateRecurrentPaymentsAutomatically = false;
            terminalDetails.Settings.VATExempt = VatExempt;

            var clientupdate = new RestClient(string.Format("https://ecng-merchants.azurewebsites.net/api/terminals/{0}", TerminalID));
            client.Timeout = -1;
            var requestupdate = new RestRequest(Method.PUT);
            requestupdate.AddHeader("Content-Type", "application/json");
            requestupdate.AddHeader("Authorization", string.Format("Bearer {0}", Common.Helpers.Authorization.GetAuthorizationCode()));
            string jsonUpdateReq = JsonConvert.SerializeObject(terminalDetails);
            requestupdate.AddParameter("application/json", jsonUpdateReq, ParameterType.RequestBody);
            IRestResponse responseupdate = clientupdate.Execute(requestupdate);
            return responseupdate.StatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
