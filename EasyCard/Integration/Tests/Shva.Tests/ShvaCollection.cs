using Shva.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Shva.Tests
{
    [CollectionDefinition("ShvaCollection")]
    public class ShvaCollection : ICollectionFixture<ShvaFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
