using Moq;
using Shared.Helpers.KeyValueStorage;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Tests.MockSetups
{
    public class KeyValueStorageMockSetup
    {
        public Mock<IKeyValueStorage<CreditCardToken>> MockObj { get; set; }

        public KeyValueStorageMockSetup(bool useDefaultSetup = true)
        {
            MockObj = new Mock<IKeyValueStorage<CreditCardToken>>();

            if (useDefaultSetup)
            {
                Setup();
            }
        }

        private void Setup()
        {
            MockObj.Setup(m => m.Get(It.IsAny<string>())).Verifiable();
        }
    }
}
