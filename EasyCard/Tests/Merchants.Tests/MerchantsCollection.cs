using Merchants.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MerchantsApi.Tests
{
    /// <summary>
    /// Used for all MerchantApi ingeration tests. Contains Database context and required services
    /// </summary>
    [CollectionDefinition("MerchantsCollection")]
    public class MerchantsCollection : ICollectionFixture<MerchantsFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
