using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServerClient
{
    public class UserManagementClient : IUserManagementClient
    {
        private readonly IWebApiClient webApiClient;
        private readonly UserManagementClientSettings configuration;
        private readonly ILogger logger;

        public UserManagementClient(IWebApiClient webApiClient, ILogger<UserManagementClient> logger, IOptions<UserManagementClientSettings> configuration)
        {
            this.webApiClient = webApiClient;
            this.logger = logger;
            this.configuration = configuration.Value;
        }

        public async Task<UserOperationResponse> CreateUser(CreateUserRequestModel model)
        {
            try
            {
                return await webApiClient.Post<UserOperationResponse>(configuration.IdentityServerAddress, "api/userManagement/user", model);
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError(clientError.Message);
                return clientError.TryConvert(new UserOperationResponse { Message = clientError.Message });
            }
        }

        public Task<UserOperationResponse> DeleteUser(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<UserProfileDataResponse> GetUserByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<UserProfileDataResponse> GetUserByID(string userId)
        {
            try
            {
                return await webApiClient.Get<UserProfileDataResponse>(configuration.IdentityServerAddress, "api/userManagement/user", new { userID = userId });
            }
            catch (WebApiClientErrorException clientError) when (clientError.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new ApplicationException($"User {userId} does not exist");
            }
        }

        public async Task<UserOperationResponse> LockUser(string userId)
        {
            try
            {
                return await webApiClient.Post<UserOperationResponse>(configuration.IdentityServerAddress, $"api/userManagement/user{userId}/lock", null);
            }
            catch (WebApiClientErrorException clientError) when (clientError.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogError($"Cannot lock user. User {userId} does not exist");
                throw new ApplicationException($"User {userId} does not exist");
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError(clientError.Message);
                return clientError.TryConvert(new UserOperationResponse { Message = clientError.Message });
            }
        }

        public Task<UserOperationResponse> ResetPassword(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<UserOperationResponse> UnLockUser(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
