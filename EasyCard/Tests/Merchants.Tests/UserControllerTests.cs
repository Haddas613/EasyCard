using Merchants.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Extensions.Ordering;

namespace MerchantsApi.Tests
{
    [Collection("MerchantsCollection"), Order(3)]
    public class UserControllerTests
    {
        private MerchantsFixture merchantsFixture;

        public UserControllerTests(MerchantsFixture merchantsFixture)
        {
            this.merchantsFixture = merchantsFixture;
        }

       
    }
}
