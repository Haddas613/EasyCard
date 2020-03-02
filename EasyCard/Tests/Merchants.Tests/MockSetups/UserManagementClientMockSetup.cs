using IdentityServerClient;
using Merchants.Api.Models.User;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Tests.MockSetups
{
    public class UserManagementClientMockSetup
    {
        public Mock<IUserManagementClient> MockObj { get; private set; }

        /// <summary>
        /// This user is always present in the list (unless deleted) and can be used as reference for tests
        /// </summary>
        public readonly string UserEntityId = Guid.NewGuid().ToString();

        /// <summary>
        /// This user is always present in the list (unless deleted) and can be used as reference for tests
        /// </summary>
        public readonly string UserEmail = Guid.NewGuid().ToString();

        public UserManagementClientMockSetup()
        {
            MockObj = new Mock<IUserManagementClient>();
            Setup();
        }

        private void Setup()
        {
            MockObj.Setup(m => m.GetUserByEmail(UserEmail))
                .Returns(Task.FromResult(new UserProfileDataResponse { UserID = UserEntityId, Email = UserEmail }))
                .Verifiable();

            MockObj.Setup(m => m.GetUserByEmail(It.IsNotIn(new string[] { UserEmail })))
                .Returns(Task.FromResult<UserProfileDataResponse>(null))
                .Verifiable();

            MockObj.Setup(m => m.CreateUser(It.IsAny<CreateUserRequestModel>()))
                .Returns(Task.FromResult(new UserOperationResponse { EntityReference = UserEntityId }))
                .Verifiable();

            MockObj.Setup(m => m.DeleteUser(UserEntityId))
                .Returns(Task.FromResult(new UserOperationResponse { EntityReference = UserEntityId }))
                .Verifiable();

            MockObj.Setup(m => m.LockUser(UserEntityId))
                .Returns(Task.FromResult(new UserOperationResponse { EntityReference = UserEntityId }))
                .Verifiable();

            MockObj.Setup(m => m.ResetPassword(UserEntityId))
                .Returns(Task.FromResult(new UserOperationResponse { EntityReference = UserEntityId }))
                .Verifiable();

            MockObj.Setup(m => m.UnLockUser(UserEntityId))
                .Returns(Task.FromResult(new UserOperationResponse { EntityReference = UserEntityId }))
                .Verifiable();
        }
    }
}
