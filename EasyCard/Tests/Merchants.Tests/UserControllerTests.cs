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

        [Fact(DisplayName = "LockUser: Locks when model is correct"), Order(3)]
        public async Task LockUser_LocksWhenModelIsCorrect()
        {
            var clientMockSetup = new UserManagementClientMockSetup();
            var controller = new UserApiController(clientMockSetup.MockObj.Object);

            var actionResult = await controller.LockUser(clientMockSetup.UserEmail);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);
            clientMockSetup.MockObj.Verify(m => m.GetUserByEmail(clientMockSetup.UserEmail), Times.Once);
            clientMockSetup.MockObj.Verify(m => m.LockUser(It.IsAny<string>()), Times.Once);
        }

        [Fact(DisplayName = "UnLockUser: Unlocks when model is correct"), Order(4)]
        public async Task UnLockUser_UnlocksWhenModelIsCorrect()
        {
            var clientMockSetup = new UserManagementClientMockSetup();
            var controller = new UserApiController(clientMockSetup.MockObj.Object);
            var userEmail = Guid.NewGuid().ToString();

            var actionResult = await controller.UnLockUser(userEmail);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);
            clientMockSetup.MockObj.Verify(m => m.GetUserByEmail(userEmail), Times.Once);
            clientMockSetup.MockObj.Verify(m => m.UnLockUser(It.IsAny<string>()), Times.Once);
        }

        [Fact(DisplayName = "ResetPassword: Resets when model is correct"), Order(5)]
        public async Task ResetPassword_ResetsWhenModelIsCorrect()
        {
            var clientMockSetup = new UserManagementClientMockSetup();
            var controller = new UserApiController(clientMockSetup.MockObj.Object);

            var actionResult = await controller.ResetPasswordForUser(clientMockSetup.UserEmail);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);
            clientMockSetup.MockObj.Verify(m => m.GetUserByEmail(clientMockSetup.UserEmail), Times.Once);
            clientMockSetup.MockObj.Verify(m => m.ResetPassword(It.IsAny<string>()), Times.Once);
        }

        [Fact(DisplayName = "LinkToTerminal: Links user to terminal"), Order(6)]
        public async Task LinkToTerminal_LinksUserToTerminal()
        {
            var clientMockSetup = new UserManagementClientMockSetup();
            var controller = new UserApiController(clientMockSetup.MockObj.Object);

            //Get terminal ID which is guaranteed to be not linked to current user
            var terminalID = (await merchantsFixture.MerchantsContext.UserTerminalMappings.Where(u => u.UserID != clientMockSetup.UserEntityId).FirstOrDefaultAsync())?.TerminalID 
                ?? await merchantsFixture.MerchantsContext.Terminals.Select(s => s.TerminalID).FirstAsync();

            var actionResult = await controller.LinkUserToTerminal(clientMockSetup.UserEmail, terminalID);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as OperationResponse;
            var linkedTerminal = await merchantsFixture.MerchantsContext.UserTerminalMappings.FirstOrDefaultAsync(t => t.TerminalID == terminalID && t.UserID == clientMockSetup.UserEntityId);

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);
            clientMockSetup.MockObj.Verify(m => m.GetUserByEmail(clientMockSetup.UserEmail), Times.Once);
        }

        [Fact(DisplayName = "UnlinkFromTerminal: UnLinks user to terminal"), Order(7)]
        public async Task UnlinkFromTerminal_UnLinksUserToTerminal()
        {
            var clientMockSetup = new UserManagementClientMockSetup();
            var controller = new UserApiController(clientMockSetup.MockObj.Object);

            //Get terminal ID which is guaranteed to be not linked to current user
            var terminalID = (await merchantsFixture.MerchantsContext.UserTerminalMappings.Where(u => u.UserID != clientMockSetup.UserEntityId).FirstOrDefaultAsync())?.TerminalID
                ?? throw new Exception("There is no linked terminals");

            var actionResult = await controller.UnlinkUserFromTerminal(clientMockSetup.UserEmail, terminalID);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as OperationResponse;
            var linkedTerminal = await merchantsFixture.MerchantsContext.UserTerminalMappings
                .FirstOrDefaultAsync(t => t.TerminalID == terminalID && t.UserID == clientMockSetup.UserEntityId);

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);
            clientMockSetup.MockObj.Verify(m => m.GetUserByEmail(clientMockSetup.UserEmail), Times.Once);
        }
    }
}
