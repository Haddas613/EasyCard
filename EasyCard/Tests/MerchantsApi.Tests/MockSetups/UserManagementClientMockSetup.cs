using IdentityServerClient;
using MerchantsApi.Models.User;
using Moq;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MerchantsApi.Tests.Mocks
{
    public class UserManagementClientMockSetup
    {
        public Mock<IUserManagementClient> MockObj { get; private set; }

        /// <summary>
        /// This user is always present in the list (unless deleted) and can be used as reference for tests
        /// </summary>
        public readonly string UserEntityId = Guid.NewGuid().ToString();

        public UserManagementClientMockSetup()
        {
            MockObj = new Mock<IUserManagementClient>();
            Setup();
        }

        private void Setup()
        {
            MockObj.Setup(m => m.CreateUser(It.IsAny<CreateUserRequestModel>()))
                .Returns(Task.FromResult(new UserOperationResponse { EntityReference = UserEntityId }))
                .Verifiable();

            MockObj.Setup(m => m.DeleteUser(UserEntityId))
                .Returns(Task.FromResult(new UserOperationResponse { }))
                .Verifiable();

            MockObj.Setup(m => m.LockUser(UserEntityId))
                .Returns(Task.FromResult(new UserOperationResponse { }))
                .Verifiable();

            MockObj.Setup(m => m.ResetPassword(UserEntityId))
                .Returns(Task.FromResult(new UserOperationResponse { }))
                .Verifiable();

            MockObj.Setup(m => m.UnLockUser(UserEntityId))
                .Returns(Task.FromResult(new UserOperationResponse { }))
                .Verifiable();
        }
    }
}
