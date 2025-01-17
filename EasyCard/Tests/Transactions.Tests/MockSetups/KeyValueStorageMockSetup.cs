﻿using Moq;
using Shared.Helpers;
using Shared.Helpers.KeyValueStorage;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Api.Models.Tokens;

namespace Transactions.Tests.MockSetups
{
    public class KeyValueStorageMockSetup
    {
        public Mock<IKeyValueStorage<CreditCardTokenKeyVault>> MockObj { get; set; }

        public CreditCardTokenKeyVault DefaultToken { get; private set; }

        public KeyValueStorageMockSetup(bool useDefaultSetup = true)
        {
            MockObj = new Mock<IKeyValueStorage<CreditCardTokenKeyVault>>();

            if (useDefaultSetup)
            {
                DefaultToken = new CreditCardTokenKeyVault
                {
                    CardExpiration = new CardExpiration() { Month = 10, Year = 25 },
                    CardNumber = "1111222233334444",
                    TerminalID = Guid.NewGuid(),
                    MerchantID = Guid.NewGuid()
                };

                Setup();
            }
        }

        private void Setup()
        {
            MockObj.Setup(m => m.Get(It.IsAny<string>())).ReturnsAsync(DefaultToken)
            .Verifiable();
        }
    }
}
