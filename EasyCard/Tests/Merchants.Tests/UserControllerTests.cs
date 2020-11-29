using IdentityServerClient;
using Merchants.Api.Controllers;
using Merchants.Api.Models.User;
using Merchants.Business.Entities.User;
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
    [Collection("MerchantsCollection")]
    [Order(3)]
    public class UserControllerTests
    {
        private MerchantsFixture merchantsFixture;

        public UserControllerTests(MerchantsFixture merchantsFixture)
        {
            this.merchantsFixture = merchantsFixture;
        }

        [Fact(DisplayName = "InviteUser: Invites when user exists and model is correct")]
        [Order(1)]
        public async Task InviteUser_InvitesWhenUserExistsModelIsCorrect()
        {
            var clientMockSetup = new UserManagementClientMockSetup();
            var controller = new UserApiController(merchantsFixture.TerminalsService, clientMockSetup.MockObj.Object, merchantsFixture.Mapper, null);  // TODO: mock
            var userEmail = Guid.NewGuid().ToString();
            var terminal = (await merchantsFixture.MerchantsContext.Terminals.FirstOrDefaultAsync())
                ?? throw new Exception("There is no terminals");

            var actionResult = await controller.InviteUser(new InviteUserRequest { Email = userEmail, TerminalID = terminal.TerminalID });

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);
            clientMockSetup.MockObj.Verify(m => m.CreateUser(It.IsAny<CreateUserRequestModel>()), Times.Never);
        }

        [Fact(DisplayName = "LockUser: Locks when model is correct")]
        [Order(3)]
        public async Task LockUser_LocksWhenModelIsCorrect()
        {
            var clientMockSetup = new UserManagementClientMockSetup();
            var controller = new UserApiController(merchantsFixture.TerminalsService, clientMockSetup.MockObj.Object, merchantsFixture.Mapper, null); // TODO: mock

            var actionResult = await controller.LockUser(clientMockSetup.UserEntityId);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);
            clientMockSetup.MockObj.Verify(m => m.LockUser(clientMockSetup.UserEntityId), Times.Once);
        }

        [Fact(DisplayName = "UnLockUser: Unlocks when model is correct")]
        [Order(4)]
        public async Task UnLockUser_UnlocksWhenModelIsCorrect()
        {
            var clientMockSetup = new UserManagementClientMockSetup();
            var controller = new UserApiController(merchantsFixture.TerminalsService, clientMockSetup.MockObj.Object, merchantsFixture.Mapper, null);  // TODO: mock

            var actionResult = await controller.UnLockUser(clientMockSetup.UserEntityId);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);
            clientMockSetup.MockObj.Verify(m => m.UnLockUser(clientMockSetup.UserEntityId), Times.Once);
        }

        [Fact(DisplayName = "ResetPassword: Resets when model is correct")]
        [Order(5)]
        public async Task ResetPassword_ResetsWhenModelIsCorrect()
        {
            var clientMockSetup = new UserManagementClientMockSetup();
            var controller = new UserApiController(merchantsFixture.TerminalsService, clientMockSetup.MockObj.Object, merchantsFixture.Mapper, null); // TODO: mock

            var actionResult = await controller.ResetPasswordForUser(clientMockSetup.UserEntityId);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);
            clientMockSetup.MockObj.Verify(m => m.ResetPassword(clientMockSetup.UserEntityId), Times.Once);
        }

        //[Fact(DisplayName = "LinkToTerminal: Links user to terminal")]
        //[Order(6)]
        //public async Task LinkToTerminal_LinksUserToTerminal()
        //{
        //    var clientMockSetup = new UserManagementClientMockSetup();
        //    var controller = new UserApiController(merchantsFixture.TerminalsService, clientMockSetup.MockObj.Object, merchantsFixture.Mapper, null);  // TODO: mock

        //    //Get terminal ID which is guaranteed to be not linked to current user
        //    var terminal = (await merchantsFixture.MerchantsContext.UserTerminalMappings.Where(u => u.UserID != clientMockSetup.UserEntityId).Select(d => d.Terminal).FirstOrDefaultAsync())
        //        ?? await merchantsFixture.MerchantsContext.Terminals.FirstAsync();

        //    var userInfo = new UserInfo { UserID = clientMockSetup.UserEntityId, Email = clientMockSetup.UserEmail };

        //    var request = new LinkUserToTerminalRequest { TerminalID = terminal.TerminalID };

        //    var actionResult = await controller.LinkUserToTerminal(clientMockSetup.UserEntityId, request);

        //    var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
        //    var responseData = response.Value as OperationResponse;
        //    var linkedTerminal = await merchantsFixture.MerchantsContext.UserTerminalMappings.FirstOrDefaultAsync(t => t.TerminalID == terminal.TerminalID && t.UserID == clientMockSetup.UserEntityId);

        //    Assert.NotNull(response);
        //    Assert.Equal(200, response.StatusCode);
        //    Assert.NotNull(responseData);
        //    Assert.Equal(StatusEnum.Success, responseData.Status);
        //    Assert.NotNull(responseData.Message);
        //    Assert.NotNull(linkedTerminal);
        //    clientMockSetup.MockObj.Verify(m => m.GetUserByID(clientMockSetup.UserEntityId), Times.Once);
        //}

        //[Fact(DisplayName = "UnlinkFromTerminal: UnLinks user to terminal")]
        //[Order(7)]
        //public async Task UnlinkFromTerminal_UnLinksUserToTerminal()
        //{
        //    var clientMockSetup = new UserManagementClientMockSetup();
        //    var controller = new UserApiController(merchantsFixture.TerminalsService, clientMockSetup.MockObj.Object, merchantsFixture.Mapper, null);  // TODO: mock

        //    var terminal = (await merchantsFixture.MerchantsContext.UserTerminalMappings.FirstOrDefaultAsync())
        //        ?? throw new Exception("There is no linked terminals");

        //    var actionResult = await controller.UnlinkUserFromTerminal(terminal.UserID, terminal.TerminalID);

        //    var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
        //    var responseData = response.Value as OperationResponse;
        //    var linkedTerminal = await merchantsFixture.MerchantsContext.UserTerminalMappings
        //        .FirstOrDefaultAsync(t => t.TerminalID == terminal.TerminalID && t.UserID == terminal.UserID);

        //    Assert.NotNull(response);
        //    Assert.Equal(200, response.StatusCode);
        //    Assert.NotNull(responseData);
        //    Assert.Equal(StatusEnum.Success, responseData.Status);
        //    Assert.NotNull(responseData.Message);
        //    Assert.Null(linkedTerminal);
        //    clientMockSetup.MockObj.Verify(m => m.GetUserByID(terminal.UserID), Times.Once);
        //}
    }
}
