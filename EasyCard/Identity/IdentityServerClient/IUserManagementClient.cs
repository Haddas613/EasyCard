using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServerClient
{
    public interface IUserManagementClient
    {
        Task<UserOperationResponse> CreateUser(CreateUserRequestModel model);

        Task<UserProfileDataResponse> GetUserByEmail(string email);

        Task<UserProfileDataResponse> GetUserByID(Guid id);

        Task<UserOperationResponse> DeleteUser(Guid userId);

        Task<UserOperationResponse> ResetPassword(Guid userId);

        Task<UserOperationResponse> LockUser(Guid userId);

        Task<UserOperationResponse> UnLockUser(Guid userId);
    }
}
