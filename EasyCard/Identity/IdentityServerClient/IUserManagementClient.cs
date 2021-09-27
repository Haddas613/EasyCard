using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServerClient
{
    public interface IUserManagementClient
    {
        Task<UserOperationResponse> CreateUser(CreateUserRequestModel model);

        Task<UserOperationResponse> UpdateUser(UpdateUserRequestModel model);

        Task<UserOperationResponse> ResendInvitation(ResendInvitationRequestModel model);

        Task<UserProfileDataResponse> GetUserByEmail(string email);

        Task<UserProfileDataResponse> GetUserByID(Guid id);

        Task<UserOperationResponse> DeleteUser(Guid userId);

        Task<UserOperationResponse> ResetPassword(Guid userId);

        Task<UserOperationResponse> LockUser(Guid userId);

        Task<UserOperationResponse> UnLockUser(Guid userId);

        Task<ApiKeyOperationResponse> CreateTerminalApiKey(CreateTerminalApiKeyRequest model);

        Task<ApiKeyOperationResponse> GetTerminalApiKey(Guid terminalID);

        Task<ApiKeyOperationResponse> DeleteTerminalApiKey(Guid terminalID);

        Task<UserOperationResponse> UnlinkUserFromMerchant(Guid userId, Guid merchantId);
    }
}
