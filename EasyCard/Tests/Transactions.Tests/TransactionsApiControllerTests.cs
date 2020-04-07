using Merchants.Business.Entities.Terminal;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Business.Exceptions;
using Shared.Helpers;
using Shared.Helpers.KeyValueStorage;
using Shared.Integration;
using Shared.Integration.Models;
using Shared.Tests.Fixtures;
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

        [Fact(DisplayName = "CreateTransactionWithToken: Creates when model is correct")]
        [Order(1)]
        public async Task CreateTransactionWithToken_CreatesWhenModelIsCorrect()
        {
            var (procResolverMock, aggrResolverMock, keyValueStorageMock)
                = (new ProcessorResolverMockSetup(), new AggregatorResolverMockSetup(), new KeyValueStorageMockSetup());

            keyValueStorageMock.DefaultToken.TerminalID = transactionsFixture.TerminalsServiceMockSetup.TerminalsList.First().TerminalID;

            var cardTokenController = new CardTokenController(transactionsFixture.TransactionsService, transactionsFixture.CreditCardTokenService,
                keyValueStorageMock.MockObj.Object, transactionsFixture.Mapper, transactionsFixture.TerminalsServiceMockSetup.MockObj.Object);
            var tokenRequest = new TokenRequest
            {
                CardExpiration = new CardExpiration { Month = 1, Year = 25 },
                CardNumber = "1111222233334444",
                TerminalID = transactionsFixture.HttpContextAccessorWrapper.TerminalIdClaimValue
            };
            await cardTokenController.CreateToken(tokenRequest); //To ensure that there is available token

            var controller = GetAuthorizedController(keyValueStorageMock, aggrResolverMock, transactionsFixture, procResolverMock);

            var existingToken = (await transactionsFixture.TransactionsContext.CreditCardTokenDetails.FirstOrDefaultAsync())
                ?? throw new Exception("No existing token was found");

            var transactionRequest = new CreateTransactionRequest
            {
                CreditCardToken = existingToken.CreditCardTokenID.ToString(),
                TerminalID = existingToken.TerminalID.Value,
                TransactionType = Shared.Enums.TransactionTypeEnum.RegularDeal,
                Currency = CurrencyEnum.ILS,
                TransactionAmount = 100,
            };

            var actionResult = await controller.CreateTransaction(transactionRequest);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData.EntityReference);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

            aggrResolverMock.ResolverMock.Verify(m => m.GetAggregator(It.IsAny<TerminalExternalSystem>()), Times.Once);
            aggrResolverMock.AggregatorMock.Verify(m => m.CreateTransaction(It.IsAny<AggregatorCreateTransactionRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            aggrResolverMock.AggregatorMock.Verify(m => m.CommitTransaction(It.IsAny<AggregatorCommitTransactionRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            procResolverMock.ResolverMock.Verify(m => m.GetProcessor(It.IsAny<TerminalExternalSystem>()), Times.Once);
            procResolverMock.ProcessorMock.Verify(
                m => m.CreateTransaction(It.IsAny<ProcessorCreateTransactionRequest>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);

            var transactionEntry = await transactionsFixture.TransactionsContext.PaymentTransactions
               .FirstOrDefaultAsync(t => t.TerminalID == transactionRequest.TerminalID && t.PaymentTransactionID.ToString() == responseData.EntityReference);

            Assert.NotNull(transactionEntry);

            var historyEntries = await transactionsFixture.TransactionsContext.TransactionHistories.Where(h => h.PaymentTransactionID == transactionEntry.PaymentTransactionID)
                .ToListAsync();

            Assert.True(historyEntries.Count > 0);
        }

        [Fact(DisplayName = "CreateTransactionWithCreditCard: Creates when model is correct")]
        [Order(2)]
        public async Task CreateTransactionWithCreditCard_CreatesWhenModelIsCorrect()
        {
            var (procResolverMock, aggrResolverMock, keyValueStorageMock)
                = (new ProcessorResolverMockSetup(), new AggregatorResolverMockSetup(), new KeyValueStorageMockSetup());

            var controller = GetAuthorizedController(keyValueStorageMock, aggrResolverMock, transactionsFixture, procResolverMock);

            var transactionRequest = new CreateTransactionRequest
            {
                CreditCardSecureDetails = new Api.Models.Transactions.CreditCardSecureDetails
                {
                    CardExpiration = new CardExpiration { Month = 1, Year = 25 },
                    CardNumber = "1111222233334444",
                    CardOwnerName = "TEST",
                    Cvv = "123",
                    CardOwnerNationalID = "12345678"
                },
                TerminalID = transactionsFixture.HttpContextAccessorWrapper.TerminalIdClaimValue,
                TransactionType = Shared.Enums.TransactionTypeEnum.RegularDeal,
                Currency = CurrencyEnum.ILS,
                TransactionAmount = 100,
            };

            var actionResult = await controller.CreateTransaction(transactionRequest);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData.EntityReference);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

            aggrResolverMock.ResolverMock.Verify(m => m.GetAggregator(It.IsAny<TerminalExternalSystem>()), Times.Once);
            aggrResolverMock.AggregatorMock.Verify(m => m.CreateTransaction(It.IsAny<AggregatorCreateTransactionRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            aggrResolverMock.AggregatorMock.Verify(m => m.CommitTransaction(It.IsAny<AggregatorCommitTransactionRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            procResolverMock.ResolverMock.Verify(m => m.GetProcessor(It.IsAny<TerminalExternalSystem>()), Times.Once);
            procResolverMock.ProcessorMock.Verify(
                m => m.CreateTransaction(It.IsAny<ProcessorCreateTransactionRequest>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);

            var transactionEntry = await transactionsFixture.TransactionsContext.PaymentTransactions
               .FirstOrDefaultAsync(t => t.TerminalID == transactionRequest.TerminalID && t.PaymentTransactionID.ToString() == responseData.EntityReference);

            Assert.NotNull(transactionEntry);

            var historyEntries = await transactionsFixture.TransactionsContext.TransactionHistories.Where(h => h.PaymentTransactionID == transactionEntry.PaymentTransactionID)
                .ToListAsync();

            Assert.True(historyEntries.Count > 0);
        }

        [Fact(DisplayName = "CreateTransactionWithToken: Returns error when token does not exist")]
        [Order(3)]
        public async Task CreateTransactionWithToken_ReturnsErrorWhenTokenDoesNotExist()
        {
            var (procResolverMock, aggrResolverMock, keyValueStorageMock)
                = (new ProcessorResolverMockSetup(), new AggregatorResolverMockSetup(), new KeyValueStorageMockSetup());

            var nonExistingKey = Guid.NewGuid().ToString();
            keyValueStorageMock.MockObj.Setup(m => m.Get(nonExistingKey)).Returns(Task.FromResult<CreditCardTokenKeyVault>(null));

            var controller = GetAuthorizedController(keyValueStorageMock, aggrResolverMock, transactionsFixture, procResolverMock);

            var transactionRequest = new CreateTransactionRequest
            {
                CreditCardToken = nonExistingKey,
                TerminalID = Guid.NewGuid()
            };

            await Assert.ThrowsAsync<EntityNotFoundException>(() => controller.CreateTransaction(transactionRequest));
            keyValueStorageMock.MockObj.Verify(m => m.Get(nonExistingKey), Times.Once);
        }

        [Fact(DisplayName = "CreateTransactionWithToken: Returns error when aggregator fails")]
        [Order(4)]
        public async Task CreateTransactionWithToken_ReturnsErrorWhenAggregatorFails()
        {
            var (procResolverMock, aggrResolverMock, keyValueStorageMock)
                = (new ProcessorResolverMockSetup(), new AggregatorResolverMockSetup(), new KeyValueStorageMockSetup());

            keyValueStorageMock.DefaultToken.TerminalID = transactionsFixture.TerminalsServiceMockSetup.TerminalsList.First().TerminalID;

            if (await transactionsFixture.TransactionsContext.CreditCardTokenDetails.CountAsync() == 0)
            {
                var cardTokenController = new CardTokenController(transactionsFixture.TransactionsService, transactionsFixture.CreditCardTokenService,
                    keyValueStorageMock.MockObj.Object, transactionsFixture.Mapper, transactionsFixture.TerminalsServiceMockSetup.MockObj.Object);
                var tokenRequest = new TokenRequest
                {
                    CardExpiration = new CardExpiration { Month = 1, Year = 25 },
                    CardNumber = "1111222233334444"
                };
                await cardTokenController.CreateToken(tokenRequest);
            }

            //Ensure that aggregator will not successfully commit transaction
            aggrResolverMock.AggregatorMock.Setup(m => m.CommitTransaction(It.IsAny<AggregatorCommitTransactionRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AggregatorCommitTransactionResponse { Success = false, ErrorMessage = "something is wrong" })
                .Verifiable();

            var controller = GetAuthorizedController(keyValueStorageMock, aggrResolverMock, transactionsFixture, procResolverMock);

            var existingToken = (await transactionsFixture.TransactionsContext.CreditCardTokenDetails.FirstOrDefaultAsync())
                ?? throw new Exception("No existing token was found");

            var transactionRequest = new CreateTransactionRequest
            {
                CreditCardToken = existingToken.CreditCardTokenID.ToString(),
                TerminalID = existingToken.TerminalID.Value,
                TransactionType = Shared.Enums.TransactionTypeEnum.RegularDeal,
                Currency = CurrencyEnum.ILS,
                TransactionAmount = 100,
            };

            var actionResult = await controller.CreateTransaction(transactionRequest);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.Equal(400, response.StatusCode);
            Assert.Equal(StatusEnum.Error, responseData.Status);
            Assert.NotNull(responseData.Message);

            aggrResolverMock.ResolverMock.Verify(m => m.GetAggregator(It.IsAny<TerminalExternalSystem>()), Times.Once);
            aggrResolverMock.AggregatorMock.Verify(m => m.CreateTransaction(It.IsAny<AggregatorCreateTransactionRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            aggrResolverMock.AggregatorMock.Verify(m => m.CommitTransaction(It.IsAny<AggregatorCommitTransactionRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            var transactionEntry = await transactionsFixture.TransactionsContext.PaymentTransactions
                .FirstOrDefaultAsync(t => t.TerminalID == transactionRequest.TerminalID && t.PaymentTransactionID.ToString() == responseData.EntityReference);

            Assert.NotNull(transactionEntry);

            var historyEntries = await transactionsFixture.TransactionsContext.TransactionHistories.Where(h => h.PaymentTransactionID == transactionEntry.PaymentTransactionID)
                .ToListAsync();

            // TODO: also check if there is entries with unsuccessful status
            Assert.True(historyEntries.Count() > 0);
        }

        [Fact(DisplayName = "CreateTransactionWithToken: Returns error when processor fails")]
        [Order(5)]
        public async Task CreateTransactionWithToken_ReturnsErrorWhenProcessorFails()
        {
            var (procResolverMock, aggrResolverMock, keyValueStorageMock)
                = (new ProcessorResolverMockSetup(), new AggregatorResolverMockSetup(), new KeyValueStorageMockSetup());

            keyValueStorageMock.DefaultToken.TerminalID = transactionsFixture.TerminalsServiceMockSetup.TerminalsList.First().TerminalID;

            if (await transactionsFixture.TransactionsContext.CreditCardTokenDetails.CountAsync() == 0)
            {
                var cardTokenController = new CardTokenController(transactionsFixture.TransactionsService, transactionsFixture.CreditCardTokenService,
                    keyValueStorageMock.MockObj.Object, transactionsFixture.Mapper, transactionsFixture.TerminalsServiceMockSetup.MockObj.Object);
                var tokenRequest = new TokenRequest
                {
                    CardExpiration = new CardExpiration { Month = 1, Year = 25 },
                    CardNumber = "1111222233334444"
                };
                await cardTokenController.CreateToken(tokenRequest);
            }

            //Ensure that processor will not successfully create transaction
            procResolverMock.ProcessorMock.Setup(m => m.CreateTransaction(It.IsAny<ProcessorCreateTransactionRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ProcessorCreateTransactionResponse { Success = false, ErrorMessage = "something is wrong" })
                .Verifiable();

            var controller = GetAuthorizedController(keyValueStorageMock, aggrResolverMock, transactionsFixture, procResolverMock);

            var existingToken = (await transactionsFixture.TransactionsContext.CreditCardTokenDetails.FirstOrDefaultAsync())
                ?? throw new Exception("No existing token was found");

            var transactionRequest = new CreateTransactionRequest
            {
                CreditCardToken = existingToken.CreditCardTokenID.ToString(),
                TerminalID = existingToken.TerminalID.Value,
                TransactionType = Shared.Enums.TransactionTypeEnum.RegularDeal,
                Currency = CurrencyEnum.ILS,
                TransactionAmount = 100,
            };

            var actionResult = await controller.CreateTransaction(transactionRequest);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.Equal(400, response.StatusCode);
            Assert.Equal(StatusEnum.Error, responseData.Status);
            Assert.NotNull(responseData.Message);

            aggrResolverMock.ResolverMock.Verify(m => m.GetAggregator(It.IsAny<TerminalExternalSystem>()), Times.Once);

            procResolverMock.ResolverMock.Verify(m => m.GetProcessor(It.IsAny<TerminalExternalSystem>()), Times.Once);
            procResolverMock.ProcessorMock.Verify(
                m => m.CreateTransaction(It.IsAny<ProcessorCreateTransactionRequest>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);

            var transactionEntry = await transactionsFixture.TransactionsContext.PaymentTransactions
               .FirstOrDefaultAsync(t => t.TerminalID == transactionRequest.TerminalID && t.PaymentTransactionID.ToString() == responseData.EntityReference);

            Assert.NotNull(transactionEntry);

            var historyEntries = await transactionsFixture.TransactionsContext.TransactionHistories.Where(h => h.PaymentTransactionID == transactionEntry.PaymentTransactionID)
                .ToListAsync();

            // TODO: also check if there is entries with unsuccessful status
            Assert.True(historyEntries.Count() > 0);
        }

        [Fact(DisplayName = "GetOneTransaction: Returns existing transaction")]
        [Order(6)]
        public async Task GetOneTransaction_ReturnsExistingTransaction()
        {
            var (procResolverMock, aggrResolverMock, keyValueStorageMock)
                = (new ProcessorResolverMockSetup(), new AggregatorResolverMockSetup(), new KeyValueStorageMockSetup());

            var existingTransaction = await transactionsFixture.TransactionsContext.PaymentTransactions.FirstOrDefaultAsync()
                ?? throw new Exception("No existing transactions found");

            var controller = GetAuthorizedController(keyValueStorageMock, aggrResolverMock, transactionsFixture, procResolverMock);

            var actionResult = await controller.GetTransaction(existingTransaction.PaymentTransactionID);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as TransactionResponse;

            Assert.Equal(200, response.StatusCode);
            Assert.Equal(responseData.PaymentTransactionID, existingTransaction.PaymentTransactionID);
        }

        [Fact(DisplayName = "GetManyTransactions: Returns filtered collection")]
        [Order(7)]
        public async Task GetManyTransactions_ReturnsFilteredCollection()
        {
            var (procResolverMock, aggrResolverMock, keyValueStorageMock)
                = (new ProcessorResolverMockSetup(), new AggregatorResolverMockSetup(), new KeyValueStorageMockSetup());

            var existingTransaction = await transactionsFixture.TransactionsContext.PaymentTransactions.FirstOrDefaultAsync()
                ?? throw new Exception("No existing transactions found");

            var controller = GetAuthorizedController(keyValueStorageMock, aggrResolverMock, transactionsFixture, procResolverMock);

            var filter = new TransactionsFilter { TerminalID = existingTransaction.TerminalID };

            var actionResult = await controller.GetTransactions(filter);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as SummariesResponse<TransactionSummary>;

            Assert.Equal(200, response.StatusCode);
            Assert.True(responseData.NumberOfRecords > 0);
            Assert.True(responseData.Data.All(t => t.TerminalID == filter.TerminalID));
        }

        private TransactionsApiController GetAuthorizedController(
            KeyValueStorageMockSetup keyValueStorageMock, AggregatorResolverMockSetup aggrResolverMock,
            TransactionsFixture transactionsFixture, ProcessorResolverMockSetup procResolverMock)
        {
            var controller = new TransactionsApiController(transactionsFixture.TransactionsService, keyValueStorageMock.MockObj.Object, transactionsFixture.Mapper,
                aggrResolverMock.ResolverMock.Object, procResolverMock.ResolverMock.Object, transactionsFixture.TerminalsServiceMockSetup.MockObj.Object, transactionsFixture.Logger);

            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = transactionsFixture.HttpContextAccessorWrapper.GetUser()
                }
            };

            return controller;
        }
    }
}
