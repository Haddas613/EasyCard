using Shared.Helpers;
using Shared.Integration.Models;
using Shva.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shva.Tests.Fixtures
{
    public class ShvaFixture : IDisposable
    {
        public ShvaSettings ShvaSettings { get; private set; }

        public ILogger<ShvaProcessor> Logger { get; } = new NullLogger<ShvaProcessor>();

        public ShvaFixture()
        {
            //TODO: assign values
            ShvaSettings = new ShvaSettings { };
            ShvaSettings.BaseUrl = "";

         
            ExternalPaymentTransactionRequest shvaReq = new ExternalPaymentTransactionRequest();
            WebApiClient jh = new WebApiClient();
             ShvaProcessor shvaperoc = new ShvaProcessor(jh, ShvaSettings, null); 

            var task = Task.Run(async () => await shvaperoc.CreateTransaction(shvaReq, "", ""));
        }

        public void Dispose()
        {
            //Dispose context, clients etc..
        }
    }
}
