using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Transactions.Api.Controllers;
using Transactions.Api.Models.Transactions;
using Transactions.Tests.Fixtures;
using Transactions.Tests.MockSetups;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Transactions.Tests
{
    [Collection("TransactionsCollection")]
    [Order(1)]
    public class TransactionsApiControllerTests
    {
        private TransactionsFixture transactionsFixture;

        public TransactionsApiControllerTests(TransactionsFixture transactionsFixture)
        {
            this.transactionsFixture = transactionsFixture;
        }

        [Fact(DisplayName = "CreateToken: Creates when model is correct")]
        [Order(1)]
        public async Task CreateToken_CreatesWhenModelIsCorrect()
        {
            var keyValueStorageMock = new KeyValueStorageMockSetup().MockObj;
            var controller = new TransactionsApiController(transactionsFixture.TransactionsService, keyValueStorageMock.Object, transactionsFixture.Mapper);
            var tokenRequest = new TokenRequest
            {
                Cvv = "123",
                CardExpiration = new CardExpiration { Month = 1, Year = 25 },
                CardNumber = "1111222233334444"
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
        }
    }
}
