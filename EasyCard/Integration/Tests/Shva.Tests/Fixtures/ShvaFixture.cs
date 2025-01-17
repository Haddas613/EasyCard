﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shared.Helpers;
using Shared.Integration.Models;
using Shared.Tests.Fixtures;
using Shva.Tests.MockSetups;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shva.Tests.Fixtures
{
    public class ShvaFixture : IDisposable
    {
        public ShvaGlobalSettings ShvaSettings { get; private set; }

        public ILogger<ShvaProcessor> Logger { get; } = new NullLogger<ShvaProcessor>();

        public ShvaFixture()
        {
            //TODO: assign values
            ShvaSettings = new ShvaGlobalSettings { };
            ShvaSettings.BaseUrl = "";


            ProcessorCreateTransactionRequest shvaReq = new ProcessorCreateTransactionRequest();
            WebApiClientMockSetup webApiClient = new WebApiClientMockSetup();
            var integrationStorage = new IntegrationRequestLogStorageServiceMock();

            ShvaProcessor shvaperoc = new ShvaProcessor(webApiClient.MockObj.Object, Options.Create<ShvaGlobalSettings>(ShvaSettings), Logger, integrationStorage.MockObj.Object);

            var task = Task.Run(async () => await shvaperoc.CreateTransaction(shvaReq));
        }

        public void Dispose()
        {
            //Dispose context, clients etc..
        }
    }
}
