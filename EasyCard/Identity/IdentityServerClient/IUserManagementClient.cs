using Shared.Models;
using Shared.Models.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServerClient
{
    public interface IUserManagementClient
    {
        Task<OperationResponse> CreateUser(CreateUserRequestModel model);

        Task<OperationResponse> DeleteUser(string userId);

        Task<OperationResponse> ResetPassword(string userId);

        Task<OperationResponse> LockUser(string userId);

        Task<OperationResponse> UnLockUser(string userId);
    }
}
