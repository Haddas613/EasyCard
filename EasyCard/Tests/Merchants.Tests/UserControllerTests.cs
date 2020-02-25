using IdentityServerClient;
using Merchants.Api.Controllers;
using Merchants.Api.Models.User;
using Merchants.Tests.Fixtures;
using Merchants.Tests.MockSetups;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [Fact(DisplayName = "CreateUser: Creates when model is correct"), Order(1)]
        public async Task CreateUser_CreatesWhenModelIsCorrect()
        {
            var clientMockSetup = new UserManagementClientMockSetup();
            var controller = new UserApiController(clientMockSetup.MockObj.Object);

            var actionResult = await controller.CreateUser(new Merchants.Api.Models.User.UserRequest());

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);
            clientMockSetup.MockObj.Verify(m => m.CreateUser(It.IsAny<CreateUserRequestModel>()), Times.Once);
        }

        [Fact(DisplayName = "GetUsers: Returns any collection"), Order(2)]
        public async Task GetUsers_ReturnsAnyCollection()
        {
            var clientMockSetup = new UserManagementClientMockSetup();
            var controller = new UserApiController(clientMockSetup.MockObj.Object);

            var actionResult = await controller.GetUsers(new Merchants.Api.Models.User.GetUsersFilter());

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as SummariesResponse<UserSummary>;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.Data);
        }

        [Fact(DisplayName = "LockUser: Locks when model is correct"), Order(3)]
        public async Task LockUser_LocksWhenModelIsCorrect()
        {
            var clientMockSetup = new UserManagementClientMockSetup();
            var controller = new UserApiController(clientMockSetup.MockObj.Object);
            var userId = Guid.NewGuid().ToString();

            var actionResult = await controller.LockUser(userId);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);
            clientMockSetup.MockObj.Verify(m => m.LockUser(userId), Times.Once);
        }

        [Fact(DisplayName = "UnLockUser: Unlocks when model is correct"), Order(4)]
        public async Task UnLockUser_UnlocksWhenModelIsCorrect()
        {
            var clientMockSetup = new UserManagementClientMockSetup();
            var controller = new UserApiController(clientMockSetup.MockObj.Object);
            var userId = Guid.NewGuid().ToString();

            var actionResult = await controller.UnLockUser(userId);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);
            clientMockSetup.MockObj.Verify(m => m.UnLockUser(userId), Times.Once);
        }

        [Fact(DisplayName = "ResetPassword: Resets when model is correct"), Order(5)]
        public async Task ResetPassword_ResetsWhenModelIsCorrect()
        {
            var clientMockSetup = new UserManagementClientMockSetup();
            var controller = new UserApiController(clientMockSetup.MockObj.Object);
            var userId = Guid.NewGuid().ToString();

            var actionResult = await controller.ResetPasswordForUser(userId);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);
            clientMockSetup.MockObj.Verify(m => m.ResetPassword(userId), Times.Once);
        }

        [Fact(DisplayName = "LinkToTerminal: Links user to terminal"), Order(6)]
        public async Task LinkToTerminal_LinksUserToTerminal()
        {
            var clientMockSetup = new UserManagementClientMockSetup();
            var controller = new UserApiController(clientMockSetup.MockObj.Object);
            var userId = Guid.NewGuid().ToString();

            //Get terminal ID which is guaranteed to be not linked to current user
            var terminalID = (await merchantsFixture.MerchantsContext.UserTerminalMappings.Where(u => u.UserID != userId).FirstOrDefaultAsync())?.TerminalID 
                ?? await merchantsFixture.MerchantsContext.Terminals.Select(s => s.TerminalID).FirstAsync();

            var actionResult = await controller.LinkUserToTerminal(userId, terminalID);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as OperationResponse;
            var linkedTerminal = await merchantsFixture.MerchantsContext.UserTerminalMappings.FirstOrDefaultAsync(t => t.TerminalID == terminalID && t.UserID == userId);

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

        }
    }
}
