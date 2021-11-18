using Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Helpers
{
    public static class CreditCardToken
    {
        public static bool AddCreditCardToken(string cardNumber, int month, int year, string cardOwnerName, string cardOwnerNationalID, string consumerID, string terminalID)
        {
            CardExpiration cardExpiration = new CardExpiration() { Month = month, Year = year };
            Guid consumerIDg;
            Guid.TryParse(consumerID, out consumerIDg);
            Guid terminalIDg;
            Guid.TryParse(terminalID, out terminalIDg);
            TokenRequest token = new TokenRequest() { CardNumber = cardNumber, CardExpiration = cardExpiration, CardOwnerName = cardOwnerName, CardOwnerNationalID = cardOwnerNationalID, ConsumerID = consumerIDg, TerminalID = terminalIDg };
            string jsonAddTokenRequest = JsonConvert.SerializeObject(token);
            return Common.Helpers.RestCall.Request("https://ecng-transactions.azurewebsites.net/api/cardtokens", RestSharp.Method.PUT, jsonAddTokenRequest);
        }
    }
}
