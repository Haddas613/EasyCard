﻿using Merchants.Business.Entities.Terminal;
using Moq;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Api.Services;

namespace Transactions.Tests.MockSetups
{
    public class AggregatorResolverMockSetup
    {
        public Mock<IAggregatorResolver> ResolverMock { get; set; }

        public Mock<IAggregator> AggregatorMock { get; set; }

        public AggregatorResolverMockSetup(bool useDefaultSetup = true)
        {
            ResolverMock = new Mock<IAggregatorResolver>();
            AggregatorMock = new Mock<IAggregator>();

            if (useDefaultSetup)
            {
                Setup();
            }
        }

        private void Setup()
        {
            AggregatorMock.Setup(m => m.ShouldBeProcessedByAggregator(It.IsAny<TransactionTypeEnum>(), It.IsAny<SpecialTransactionTypeEnum>(), It.IsAny<JDealTypeEnum>()))
               .Returns(true)
               .Verifiable();

            AggregatorMock.Setup(m => m.CreateTransaction(It.IsAny<AggregatorCreateTransactionRequest>()))
                .ReturnsAsync(new AggregatorCreateTransactionResponse { Success = true })
                .Verifiable();

            AggregatorMock.Setup(m => m.CommitTransaction(It.IsAny<AggregatorCommitTransactionRequest>()))
                .ReturnsAsync(new AggregatorCommitTransactionResponse { Success = true })
                .Verifiable();

            ResolverMock.Setup(m => m.GetAggregator(It.IsAny<TerminalExternalSystem>()))
                .Returns(AggregatorMock.Object)
                .Verifiable();
        }
    }
}
