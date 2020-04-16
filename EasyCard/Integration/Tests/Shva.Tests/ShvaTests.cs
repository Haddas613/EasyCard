using Microsoft.Extensions.Options;
using Shared.Integration.Models;
using Shared.Tests.Fixtures;
using Shva.Tests.Fixtures;
using Shva.Tests.MockSetups;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Shva.Tests
{
    [Collection("ShvaCollection"), Order(3)]
    public class ShvaTests
    {
        private readonly ShvaFixture shvaFixture;

        public ShvaTests(ShvaFixture shvaFixture)
        {
            this.shvaFixture = shvaFixture;
        }

        [Fact]
        public async Task CreateBasicShvaTransaction()
        {
            var webApiClient = new WebApiClientMockSetup();
            var integrationStorage = new IntegrationRequestLogStorageServiceMock();

            var processor = new ShvaProcessor(webApiClient.MockObj.Object, Options.Create<ShvaGlobalSettings>(shvaFixture.ShvaSettings), shvaFixture.Logger, integrationStorage.MockObj.Object);

            var request = new ProcessorCreateTransactionRequest
            {
                ProcessorSettings = new ShvaTerminalSettings()
            };

            var response = await processor.CreateTransaction(request);

            var shvaResponse = response as ShvaCreateTransactionResponse;

            Assert.True(shvaResponse?.Success);
        }
    }
}
