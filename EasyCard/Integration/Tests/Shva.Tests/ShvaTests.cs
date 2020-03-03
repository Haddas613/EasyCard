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
            var clientMockSetup = new WebApiClientMockSetup();
            var processor = new ShvaProcessor(clientMockSetup.MockObj.Object, shvaFixture.ShvaSettings, null);//TODO: add ILogger mock

            
            //processor.CreateTransaction(...);

        }
    }
}
