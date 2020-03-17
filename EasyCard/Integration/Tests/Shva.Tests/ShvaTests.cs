using Microsoft.Extensions.Options;
using Shva.Configuration;
using Shva.Tests.Fixtures;
using Shva.Tests.MockSetups;
using System;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Shva.Tests
{
    [Collection("MerchantsCollection"), Order(3)]
    public class ShvaTests
    {
        private readonly ShvaFixture shvaFixture;

        public ShvaTests(ShvaFixture shvaFixture)
        {
            this.shvaFixture = shvaFixture;
        }

        [Fact]
        public void Test()
        {
            var webApiClient = new WebApiClientMockSetup();
            var processor = new ShvaProcessor(webApiClient.MockObj.Object, Options.Create<ShvaGlobalSettings>(shvaFixture.ShvaSettings), shvaFixture.Logger);
            
            //processor.CreateTransaction(...);
        }
    }
}
