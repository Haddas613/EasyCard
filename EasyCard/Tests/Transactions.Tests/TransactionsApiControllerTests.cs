using Merchants.Business.Entities.Terminal;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Business.Exceptions;
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
        public async Task CreateTransaction_CreatesWhenModelIsCorrect()
        {
            var (procResolverMock, aggrResolverMock, keyValueStorageMock, terminalSrvMock)
                = (new ProcessorResolverMockSetup(), new AggregatorResolverMockSetup(), new KeyValueStorageMockSetup().MockObj, new TerminalsServiceMockSetup());

            var cardTokenController = new CardTokenController(transactionsFixture.TransactionsService, transactionsFixture.CreditCardTokenService,
                keyValueStorageMock.Object, transactionsFixture.Mapper);
            var tokenRequest = new TokenRequest
            {
                CardExpiration = new CardExpiration { Month = 1, Year = 25 },
                CardNumber = "1111222233334444"
            };
            await cardTokenController.CreateToken(tokenRequest); //To ensure that there is available token

            var controller = new TransactionsApiController(transactionsFixture.TransactionsService, keyValueStorageMock.Object, transactionsFixture.Mapper,
                aggrResolverMock.ResolverMock.Object, procResolverMock.ResolverMock.Object, terminalSrvMock.MockObj.Object);

            var existingToken = (await transactionsFixture.TransactionsContext.CreditCardTokenDetails.FirstOrDefaultAsync())
                ?? throw new Exception("No existing token was found");

            var transactionRequest = new TransactionRequestWithToken
            {
                CardToken = existingToken.PublicKey,
                TerminalID = existingToken.TerminalID,
                TransactionType = Shared.Enums.TransactionTypeEnum.RegularDeal,
                Currency = CurrencyEnum.ILS,
                TransactionAmount = 100,
                InstallmentPaymentAmount = 1,
                TotalAmount = 100
            };

            var actionResult = await controller.CreateTransactionWithToken(transactionRequest);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.EntityID);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

            aggrResolverMock.ResolverMock.Verify(m => m.GetAggregator(It.IsAny<Terminal>()), Times.Once);
            aggrResolverMock.AggregatorMock.Verify(m => m.CreateTransaction(It.IsAny<AggregatorCreateTransactionRequest>()), Times.Once);
            aggrResolverMock.AggregatorMock.Verify(m => m.CommitTransaction(It.IsAny<AggregatorCommitTransactionRequest>()), Times.Once);

            procResolverMock.ResolverMock.Verify(m => m.GetProcessor(It.IsAny<Terminal>()), Times.Once);
            procResolverMock.ProcessorMock.Verify(
                m => m.CreateTransaction(It.IsAny<ProcessorTransactionRequest>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);

            var transactionEntry = await transactionsFixture.TransactionsContext.PaymentTransactions
               .FirstOrDefaultAsync(t => t.TerminalID == transactionRequest.TerminalID && t.TransactionNumber == responseData.EntityID);

            Assert.NotNull(transactionEntry);

            var historyEntries = await transactionsFixture.TransactionsContext.TransactionHistories.Where(h => h.TransactionHistoryID == transactionEntry.PaymentTransactionID)
                .ToListAsync();

            Assert.True(historyEntries.Count > 0);
        }

        [Fact(DisplayName = "CreateTransaction: Returns error when token does not exist")]
        [Order(2)]
        public async Task CreateTransaction_ReturnsErrorWhenTokenDoesNotExist()
        {
            var (procResolverMock, aggrResolverMock, keyValueStorageMock, terminalSrvMock)
                = (new ProcessorResolverMockSetup(), new AggregatorResolverMockSetup(), new KeyValueStorageMockSetup().MockObj, new TerminalsServiceMockSetup());

            var nonExistingKey = Guid.NewGuid().ToString();
            keyValueStorageMock.Setup(m => m.Get(nonExistingKey)).Returns(Task.FromResult<CreditCardTokenKeyVault>(null));

            var controller = new TransactionsApiController(transactionsFixture.TransactionsService, keyValueStorageMock.Object, transactionsFixture.Mapper,
                aggrResolverMock.ResolverMock.Object, procResolverMock.ResolverMock.Object, terminalSrvMock.MockObj.Object);

            var transactionRequest = new TransactionRequestWithToken
            {
                CardToken = nonExistingKey,
                TerminalID = 0
            };

            await Assert.ThrowsAsync<EntityNotFoundException>(() => controller.CreateTransactionWithToken(transactionRequest));
            keyValueStorageMock.Verify(m => m.Get(nonExistingKey), Times.Once);
        }

        [Fact(DisplayName = "CreateTransaction: Returns error when aggregator fails")]
        [Order(3)]
        public async Task CreateTransaction_ReturnsErrorWhenAggregatorFails()
        {
            var (procResolverMock, aggrResolverMock, keyValueStorageMock, terminalSrvMock)
                = (new ProcessorResolverMockSetup(), new AggregatorResolverMockSetup(), new KeyValueStorageMockSetup().MockObj, new TerminalsServiceMockSetup());

            if (await transactionsFixture.TransactionsContext.CreditCardTokenDetails.CountAsync() == 0)
            {
                var cardTokenController = new CardTokenController(transactionsFixture.TransactionsService, transactionsFixture.CreditCardTokenService,
                    keyValueStorageMock.Object, transactionsFixture.Mapper);
                var tokenRequest = new TokenRequest
                {
                    CardExpiration = new CardExpiration { Month = 1, Year = 25 },
                    CardNumber = "1111222233334444"
                };
                await cardTokenController.CreateToken(tokenRequest);
            }

            //Ensure that aggregator will not successfully commit transaction
            aggrResolverMock.AggregatorMock.Setup(m => m.CommitTransaction(It.IsAny<AggregatorCommitTransactionRequest>()))
                .ReturnsAsync(new AggregatorCommitTransactionResponse { Success = false, ErrorMessage = "something is wrong" })
                .Verifiable();

            var controller = new TransactionsApiController(transactionsFixture.TransactionsService, keyValueStorageMock.Object, transactionsFixture.Mapper,
                aggrResolverMock.ResolverMock.Object, procResolverMock.ResolverMock.Object, terminalSrvMock.MockObj.Object);

            var existingToken = (await transactionsFixture.TransactionsContext.CreditCardTokenDetails.FirstOrDefaultAsync())
                ?? throw new Exception("No existing token was found");

            var transactionRequest = new TransactionRequestWithToken
            {
                CardToken = existingToken.PublicKey,
                TerminalID = existingToken.TerminalID,
                TransactionType = Shared.Enums.TransactionTypeEnum.RegularDeal,
                Currency = CurrencyEnum.ILS,
                TransactionAmount = 100,
                InstallmentPaymentAmount = 1,
                TotalAmount = 100
            };

            var actionResult = await controller.CreateTransactionWithToken(transactionRequest);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(400, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Error, responseData.Status);
            Assert.NotNull(responseData.Message);

            aggrResolverMock.ResolverMock.Verify(m => m.GetAggregator(It.IsAny<Terminal>()), Times.Once);
            aggrResolverMock.AggregatorMock.Verify(m => m.CreateTransaction(It.IsAny<AggregatorCreateTransactionRequest>()), Times.Once);
            aggrResolverMock.AggregatorMock.Verify(m => m.CommitTransaction(It.IsAny<AggregatorCommitTransactionRequest>()), Times.Once);

            var transactionEntry = await transactionsFixture.TransactionsContext.PaymentTransactions
                .FirstOrDefaultAsync(t => t.TerminalID == transactionRequest.TerminalID && t.TransactionNumber == responseData.EntityID);

            Assert.NotNull(transactionEntry);

            var historyEntries = await transactionsFixture.TransactionsContext.TransactionHistories.Where(h => h.TransactionHistoryID == transactionEntry.PaymentTransactionID)
                .ToListAsync();

            // TODO: also check if there is entries with unsuccessful status
            Assert.True(historyEntries.Count() > 0);
        }

        [Fact(DisplayName = "CreateTransaction: Returns error when processor fails")]
        [Order(4)]
        public async Task CreateTransaction_ReturnsErrorWhenProcessorFails()
        {
            var (procResolverMock, aggrResolverMock, keyValueStorageMock, terminalSrvMock)
                = (new ProcessorResolverMockSetup(), new AggregatorResolverMockSetup(), new KeyValueStorageMockSetup().MockObj, new TerminalsServiceMockSetup());

            if (await transactionsFixture.TransactionsContext.CreditCardTokenDetails.CountAsync() == 0)
            {
                var cardTokenController = new CardTokenController(transactionsFixture.TransactionsService, transactionsFixture.CreditCardTokenService,
                    keyValueStorageMock.Object, transactionsFixture.Mapper);
                var tokenRequest = new TokenRequest
                {
                    CardExpiration = new CardExpiration { Month = 1, Year = 25 },
                    CardNumber = "1111222233334444"
                };
                await cardTokenController.CreateToken(tokenRequest);
            }

            //Ensure that processor will not successfully create transaction
            procResolverMock.ProcessorMock.Setup(m => m.CreateTransaction(It.IsAny<ProcessorTransactionRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ProcessorTransactionResponse { Success = false, ErrorMessage = "something is wrong" })
                .Verifiable();

            var controller = new TransactionsApiController(transactionsFixture.TransactionsService, keyValueStorageMock.Object, transactionsFixture.Mapper,
                aggrResolverMock.ResolverMock.Object, procResolverMock.ResolverMock.Object, terminalSrvMock.MockObj.Object);

            var existingToken = (await transactionsFixture.TransactionsContext.CreditCardTokenDetails.FirstOrDefaultAsync())
                ?? throw new Exception("No existing token was found");

            var transactionRequest = new TransactionRequestWithToken
            {
                CardToken = existingToken.PublicKey,
                TerminalID = existingToken.TerminalID,
                TransactionType = Shared.Enums.TransactionTypeEnum.RegularDeal,
                Currency = CurrencyEnum.ILS,
                TransactionAmount = 100,
                InstallmentPaymentAmount = 1,
                TotalAmount = 100
            };

            var actionResult = await controller.CreateTransactionWithToken(transactionRequest);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(400, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Error, responseData.Status);
            Assert.NotNull(responseData.Message);

            aggrResolverMock.ResolverMock.Verify(m => m.GetAggregator(It.IsAny<Terminal>()), Times.Once);

            procResolverMock.ResolverMock.Verify(m => m.GetProcessor(It.IsAny<Terminal>()), Times.Once);
            procResolverMock.ProcessorMock.Verify(
                m => m.CreateTransaction(It.IsAny<ProcessorTransactionRequest>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);

            var transactionEntry = await transactionsFixture.TransactionsContext.PaymentTransactions
               .FirstOrDefaultAsync(t => t.TerminalID == transactionRequest.TerminalID && t.TransactionNumber == responseData.EntityID);

            Assert.NotNull(transactionEntry);

            var historyEntries = await transactionsFixture.TransactionsContext.TransactionHistories.Where(h => h.TransactionHistoryID == transactionEntry.PaymentTransactionID)
                .ToListAsync();

            // TODO: also check if there is entries with unsuccessful status
            Assert.True(historyEntries.Count() > 0);
        }

        [Fact(DisplayName = "GetOneTransaction: Returns existing transaction")]
        [Order(5)]
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

        [Fact(DisplayName = "GetManyTransactions: Returns filtered collection")]
        [Order(6)]
        public async Task GetManyTransactions_ReturnsFilteredCollection()
        {
            var (procResolverMock, aggrResolverMock, keyValueStorageMock, terminalSrvMock)
                = (new ProcessorResolverMockSetup(), new AggregatorResolverMockSetup(), new KeyValueStorageMockSetup().MockObj, new TerminalsServiceMockSetup());

            var existingTransaction = await transactionsFixture.TransactionsContext.PaymentTransactions.FirstOrDefaultAsync()
                ?? throw new Exception("No existing transactions found");

            var controller = new TransactionsApiController(transactionsFixture.TransactionsService, keyValueStorageMock.Object, transactionsFixture.Mapper,
                aggrResolverMock.ResolverMock.Object, procResolverMock.ResolverMock.Object, terminalSrvMock.MockObj.Object);

            var filter = new TransactionsFilter { TerminalID = existingTransaction.TerminalID };

            var actionResult = await controller.GetTransactions(filter);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as SummariesResponse<TransactionSummary>;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.True(responseData.NumberOfRecords > 0);
            Assert.True(responseData.Data.All(t => t.TerminalID == filter.TerminalID));
        }
    }
}
