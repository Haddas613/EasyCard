using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Shared.Helpers.Security;
using System.Collections.Specialized;
using System.Net.Http.Headers;

namespace IdentityServerClient
{
    public class UserManagementClient : IUserManagementClient
    {
        private readonly IWebApiClient webApiClient;
        private readonly IdentityServerClientSettings configuration;
        private readonly ILogger logger;
        private readonly IWebApiClientTokenService tokenService;

        public UserManagementClient(IWebApiClient webApiClient, ILogger<UserManagementClient> logger, IOptions<IdentityServerClientSettings> configuration, IWebApiClientTokenService tokenService)
        {
            this.webApiClient = webApiClient;
            this.logger = logger;
            this.configuration = configuration.Value;
            this.tokenService = tokenService;
        }

        public async Task<UserOperationResponse> CreateUser(CreateUserRequestModel model)
        {
            try
            {
                return await webApiClient.Post<UserOperationResponse>(configuration.Authority, "api/userManagement/user", model, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError(clientError.Message);
                return clientError.TryConvert(new UserOperationResponse { Message = clientError.Message });
            }
        }

        public async Task<UserOperationResponse> ResendInvitation(ResendInvitationRequestModel model)
        {
            try
            {
                return await webApiClient.Post<UserOperationResponse>(configuration.Authority, "api/userManagement/resendInvitation", model, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError) when (clientError.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogError($"Cannot resend invitation for user. User {model.Email} does not exist");
                throw new EntityNotFoundException($"User does not exist", "User", model.Email);
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError(clientError.Message);
                return clientError.TryConvert(new UserOperationResponse { Message = clientError.Message });
            }
        }

        public Task<UserOperationResponse> DeleteUser(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task<UserProfileDataResponse> GetUserByEmail(string email)
        {
            try
            {
                return await webApiClient.Get<UserProfileDataResponse>(configuration.Authority, "api/userManagement/user", new { email }, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError) when (clientError.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<UserProfileDataResponse> GetUserByID(Guid userId)
        {
            try
            {
                return await webApiClient.Get<UserProfileDataResponse>(configuration.Authority, $"api/userManagement/user/{userId}", null, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError) when (clientError.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new EntityNotFoundException($"User does not exist", "User", userId.ToString());
            }
        }

        public async Task<UserOperationResponse> LockUser(Guid userId)
        {
            try
            {
                return await webApiClient.Post<UserOperationResponse>(configuration.Authority, $"api/userManagement/user/{userId}/lock", null, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError) when (clientError.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogError($"Cannot lock user. User {userId} does not exist");
                throw new EntityNotFoundException($"User does not exist", "User", userId.ToString());
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError(clientError.Message);
                return clientError.TryConvert(new UserOperationResponse { Message = clientError.Message });
            }
        }

        public async Task<ApiKeyOperationResponse> CreateTerminalApiKey(CreateTerminalApiKeyRequest model)
        {
            try
            {
                return await webApiClient.Post<ApiKeyOperationResponse>(configuration.Authority, $"api/terminalapikeys", model, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError(clientError.Message);
                return clientError.TryConvert(new ApiKeyOperationResponse { Message = clientError.Message });
            }
        }

        public async Task<ApiKeyOperationResponse> DeleteTerminalApiKey(Guid terminalID)
        {
            try
            {
                return await webApiClient.Delete<ApiKeyOperationResponse>(configuration.Authority, $"api/terminalapikeys/{terminalID}", BuildHeaders);
            }
            catch (WebApiClientErrorException clientError) when (clientError.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogError($"Cannot delete terminal api key. Terminal api key for {terminalID} does not exist");
                throw new EntityNotFoundException($"Terminal api key for {terminalID} does not exist", "TerminalApiKey", terminalID.ToString());
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError(clientError.Message);
                return clientError.TryConvert(new ApiKeyOperationResponse { Message = clientError.Message });
            }
        }

        public async Task<UserOperationResponse> ResetPassword(Guid userId)
        {
            try
            {
                return await webApiClient.Post<UserOperationResponse>(configuration.Authority, $"api/userManagement/user/{userId}/resetPassword", null, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError) when (clientError.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogError($"Cannot lock user. User {userId} does not exist");
                throw new EntityNotFoundException($"User does not exist", "User", userId.ToString());
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError(clientError.Message);
                return clientError.TryConvert(new UserOperationResponse { Message = clientError.Message });
            }
        }

        public async Task<UserOperationResponse> UnLockUser(Guid userId)
        {
            try
            {
                return await webApiClient.Post<UserOperationResponse>(configuration.Authority, $"api/userManagement/user/{userId}/unlock", null, BuildHeaders);
            }
            catch (WebApiClientErrorException clientError) when (clientError.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogError($"Cannot lock user. User {userId} does not exist");
                throw new EntityNotFoundException($"User does not exist", "User", userId.ToString());
            }
            catch (WebApiClientErrorException clientError)
            {
                logger.LogError(clientError.Message);
                return clientError.TryConvert(new UserOperationResponse { Message = clientError.Message });
            }
        }

        private async Task<NameValueCollection> BuildHeaders()
        {
            var token = await tokenService.GetToken();

            NameValueCollection headers = new NameValueCollection();

            if (token != null)
            {
                headers.Add("Authorization", new AuthenticationHeaderValue("Bearer", token.AccessToken).ToString());
            }

            return headers;
        }
    }
}
