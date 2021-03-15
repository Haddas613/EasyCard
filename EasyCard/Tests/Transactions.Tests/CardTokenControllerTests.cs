using Microsoft.EntityFrameworkCore;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Helpers;
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
    [Order(1)]
    public class CardTokenControllerTests
    {
        private TransactionsFixture transactionsFixture;

        public CardTokenControllerTests(TransactionsFixture transactionsFixture)
        {
            this.transactionsFixture = transactionsFixture;
        }

        [Fact(DisplayName = "CreateToken: Creates when model is correct")]
        [Order(1)]
        public async Task CreateToken_CreatesWhenModelIsCorrect()
        {
            var keyValueStorageMock = new KeyValueStorageMockSetup().MockObj;
            var controller = new CardTokenController(
                transactionsFixture.TransactionsService,
                transactionsFixture.CreditCardTokenService,
                keyValueStorageMock.Object,
                transactionsFixture.Mapper,
                transactionsFixture.TerminalsServiceMockSetup.MockObj.Object,
                transactionsFixture.AppSettings,
                null,
                null,
                null,
                null,
                null); // TODO: add fixture

            var tokenRequest = new TokenRequest
            {
                CardExpiration = new CardExpiration { Month = 1, Year = 25 },
                CardNumber = "1111222233334444",
                TerminalID = transactionsFixture.HttpContextAccessorWrapper.TerminalIdClaimValue
            };
            var actionResult = await controller.CreateToken(tokenRequest);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.EntityReference);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);
            keyValueStorageMock.Verify(m => m.Save(responseData.EntityReference, Moq.It.IsAny<string>()), Moq.Times.Once);

            var dbToken = transactionsFixture.TransactionsContext.CreditCardTokenDetails.FirstOrDefault(t => t.CreditCardTokenID.ToString() == responseData.EntityReference);
            Assert.NotNull(dbToken);
        }

        [Fact(DisplayName = "GetTokens: Returns collection of tokens")]
        [Order(2)]
        public async Task GetTokens_ReturnsCollectionOfTokens()
        {
            var keyValueStorageMock = new KeyValueStorageMockSetup().MockObj;
            var controller = new CardTokenController(
                transactionsFixture.TransactionsService,
                transactionsFixture.CreditCardTokenService,
                keyValueStorageMock.Object,
                transactionsFixture.Mapper,
                transactionsFixture.TerminalsServiceMockSetup.MockObj.Object,
                transactionsFixture.AppSettings,
                null,
                null,
                null,
                null,
                null); // TODO: add fixture
            var filter = new CreditCardTokenFilter();
            var actionResult = await controller.GetTokens(filter);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as SummariesResponse<CreditCardTokenSummary>;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.True(responseData.NumberOfRecords > 0);
        }

        [Fact(DisplayName = "DeteleToken: Deletes token")]
        [Order(3)]
        public async Task DeleteToken_DeletesToken()
        {
            var keyValueStorageMock = new KeyValueStorageMockSetup().MockObj;
            var controller = new CardTokenController(
                transactionsFixture.TransactionsService,
                transactionsFixture.CreditCardTokenService,
                keyValueStorageMock.Object,
                transactionsFixture.Mapper,
                transactionsFixture.TerminalsServiceMockSetup.MockObj.Object,
                transactionsFixture.AppSettings,
                null,
                null,
                null,
                null,
                null); // TODO: add fixture
            var existingToken = transactionsFixture.TransactionsContext.CreditCardTokenDetails.FirstOrDefault() ?? throw new Exception("No existing token found to delete");
            var actionResult = await controller.DeleteToken(existingToken.CreditCardTokenID.ToString());

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData.EntityReference);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.Null(transactionsFixture.TransactionsContext.CreditCardTokenDetails.FirstOrDefault(t => t.CreditCardTokenID == existingToken.CreditCardTokenID));
            keyValueStorageMock.Verify(m => m.Delete(existingToken.CreditCardTokenID.ToString()), Moq.Times.Once);
        }
    }
}
