using Merchants.Business.Entities.Terminal;
using Moq;
using Shared.Integration;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Api.Services;

namespace Transactions.Tests.MockSetups
{
    public class ProcessorResolverMockSetup
    {
        public Mock<IProcessorResolver> ResolverMock { get; set; }

        public Mock<IProcessor> ProcessorMock { get; set; }

        public ProcessorResolverMockSetup(bool useDefaultSetup = true)
        {
            ResolverMock = new Mock<IProcessorResolver>();
            ProcessorMock = new Mock<IProcessor>();

            if (useDefaultSetup)
            {
                Setup();
            }
        }

        private void Setup()
        {
            ProcessorMock.Setup(m => m.CreateTransaction(It.IsAny<ProcessorTransactionRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ProcessorTransactionResponse { Success = true })
                .Verifiable();

            ResolverMock.Setup(m => m.GetProcessor(It.IsAny<Terminal>()))
                .Returns(ProcessorMock.Object)
                .Verifiable();
        }
    }
}
