﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServerClient
{
    public interface IUserManagementClient
    {
        Task<UserOperationResponse> CreateUser(CreateUserRequestModel model);

        Task<UserProfileDataResponse> GetUserByEmail(string email);

        Task<UserProfileDataResponse> GetUserByID(string id);

        Task<UserOperationResponse> DeleteUser(string userId);

        Task<UserOperationResponse> ResetPassword(string userId);

        Task<UserOperationResponse> LockUser(string userId);

        Task<UserOperationResponse> UnLockUser(string userId);
    }
}
