using Merchants.Business.Entities.Terminal;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Helpers;
using Shared.Integration;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transactions.Api.Controllers;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Tests.Fixtures;
using Transactions.Tests.MockSetups;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Transactions.Tests
{
    [Collection("TransactionsCollection")]
    [Order(2)]
    public class TransactionsApiControllerTests
    {
        private TransactionsFixture transactionsFixture;

        public TransactionsApiControllerTests(TransactionsFixture transactionsFixture)
        {
            this.transactionsFixture = transactionsFixture;
        }

        [Fact(DisplayName = "CreateTransaction: Creates when model is correct")]
        [Order(1)]
        public async Task CreateToken_CreatesWhenModelIsCorrect()
        {
            var (procResolverMock, aggrResolverMock, keyValueStorageMock, terminalSrvMock)
                = (new ProcessorResolverMockSetup(), new AggregatorResolverMockSetup(), new KeyValueStorageMockSetup().MockObj, new TerminalsServiceMockSetup());

            var cardTokenController = new CardTokenController(transactionsFixture.TransactionsService, transactionsFixture.CreditCardTokenService,
                keyValueStorageMock.Object, transactionsFixture.Mapper);
            var tokenRequest = new TokenRequest
            {
                Cvv = "123",
                CardExpiration = new CardExpiration { Month = 1, Year = 25 },
                CardNumber = "1111222233334444"
            };
            await cardTokenController.CreateToken(tokenRequest); //To ensure that there is available token

            var controller = new TransactionsApiController(transactionsFixture.TransactionsService, keyValueStorageMock.Object, transactionsFixture.Mapper,
                aggrResolverMock.ResolverMock.Object, procResolverMock.ResolverMock.Object, terminalSrvMock.MockObj.Object);

            var existingToken = (await transactionsFixture.TransactionsContext.CreditCardTokenDetails.FirstOrDefaultAsync())
                ?? throw new Exception("No existing token was found");

            var transactionRequest = new TransactionRequest
            {
                CardToken = existingToken.PublicKey,
                TerminalID = existingToken.TerminalID
            };

            var actionResult = await controller.CreateTransaction(transactionRequest);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.EntityID);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

            keyValueStorageMock.Verify(m => m.Save(responseData.EntityReference, It.IsAny<string>()), Times.Once);

            aggrResolverMock.ResolverMock.Verify(m => m.GetAggregator(It.IsAny<Terminal>()), Times.Once);
            aggrResolverMock.AggregatorMock.Verify(m => m.CreateTransaction(It.IsAny<AggregatorCreateTransactionRequest>()), Times.Once);
            aggrResolverMock.AggregatorMock.Verify(m => m.CommitTransaction(It.IsAny<AggregatorCommitTransactionRequest>()), Times.Once);

            procResolverMock.ResolverMock.Verify(m => m.GetProcessor(It.IsAny<Terminal>()), Times.Once);
            procResolverMock.ProcessorMock.Verify(
                m => m.CreateTransaction(It.IsAny<ProcessorTransactionRequest>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<Func<IntegrationMessage, IntegrationMessage>>()), Times.Once);

            var historyEntries = transactionsFixture.TransactionsContext.TransactionHistories.Count(h => h.TransactionHistoryID == responseData.EntityID);
            Assert.True(historyEntries > 0);
        }

        [Fact(DisplayName = "GetOneTransaction: Returns existing transaction")]
        [Order(2)]
        public async Task GetOneTransaction_ReturnsExistingTransaction()
        {
            var (procResolverMock, aggrResolverMock, keyValueStorageMock, terminalSrvMock)
                = (new ProcessorResolverMockSetup(), new AggregatorResolverMockSetup(), new KeyValueStorageMockSetup().MockObj, new TerminalsServiceMockSetup());

            var existingTransaction = await transactionsFixture.TransactionsContext.PaymentTransactions.FirstOrDefaultAsync()
                ?? throw new Exception("No existing transactions found");

            var controller = new TransactionsApiController(transactionsFixture.TransactionsService, keyValueStorageMock.Object, transactionsFixture.Mapper,
                aggrResolverMock.ResolverMock.Object, procResolverMock.ResolverMock.Object, terminalSrvMock.MockObj.Object);

            var actionResult = await controller.GetTransaction(existingTransaction.PaymentTransactionID);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as TransactionResponse;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(responseData.PaymentTransactionID, existingTransaction.PaymentTransactionID);
        }

        [Fact(DisplayName = "CreateTransaction: Returns error when token does not exist")]
        [Order(3)]
        public async Task CreateTransaction_ReturnsErrorWhenTokenDoesNotExist()
        {
            var (procResolverMock, aggrResolverMock, keyValueStorageMock, terminalSrvMock)
                = (new ProcessorResolverMockSetup(), new AggregatorResolverMockSetup(), new KeyValueStorageMockSetup().MockObj, new TerminalsServiceMockSetup());

            var nonExistingKey = Guid.NewGuid().ToString();
            keyValueStorageMock.Setup(m => m.Get(nonExistingKey)).Returns(Task.FromResult<CreditCardTokenKeyVault>(null));

            var controller = new TransactionsApiController(transactionsFixture.TransactionsService, keyValueStorageMock.Object, transactionsFixture.Mapper,
                aggrResolverMock.ResolverMock.Object, procResolverMock.ResolverMock.Object, terminalSrvMock.MockObj.Object);

            var transactionRequest = new TransactionRequest
            {
                CardToken = nonExistingKey,
                TerminalID = 0
            };

            var actionResult = await controller.CreateTransaction(transactionRequest);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.NotEqual(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Null(responseData.EntityID);
            Assert.Equal(StatusEnum.Error, responseData.Status);
            Assert.NotNull(responseData.Message);

            keyValueStorageMock.Verify(m => m.Get(nonExistingKey), Times.Once);
        }

    }
}
