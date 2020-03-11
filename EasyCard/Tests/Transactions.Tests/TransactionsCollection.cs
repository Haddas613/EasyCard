using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Tests.Fixtures;
using Xunit;

namespace Transactions.Tests
{
    [CollectionDefinition("TransactionsCollection")]
    public class TransactionsCollection : ICollectionFixture<TransactionsFixture>
    {
    }
}
