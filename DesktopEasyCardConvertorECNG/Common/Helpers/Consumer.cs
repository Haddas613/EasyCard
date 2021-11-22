using Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Helpers
{
    public static class Consumer
    {
        public static bool AddConsumer(string terminalID, bool active, string billingDesktopRefNumber, string street, string city, string lastName, string name, string identityNumber, string note, string phone, string phone2, string externalReference, string clientName)
        {
            Guid terminalg;
            Guid.TryParse(terminalID, out terminalg);
            Address address = new Address() { Street = street, City = city };
            ConsumerRequest consumer = new ConsumerRequest() { Active = active, BillingDesktopRefNumber = billingDesktopRefNumber, ConsumerAddress = address, ConsumerName = string.Format("{0} {1}", lastName, name), ConsumerNationalID = identityNumber, ConsumerPhone = phone, ConsumerPhone2 = phone2, ConsumerNote = note, TerminalID = terminalg, ExternalReference = externalReference, Origin = string.Format("BillingDesktop {0}", clientName), ConsumerSecondPhone = phone2 };
            string jsonCreateReq = JsonConvert.SerializeObject(consumer);
            return Common.Helpers.RestCall.Request("https://ecng-profile.azurewebsites.net/api/consumers", RestSharp.Method.PUT, jsonCreateReq);
        }
    }
}
