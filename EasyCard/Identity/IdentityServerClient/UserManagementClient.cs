﻿using Microsoft.Extensions.Logging;
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

        public Task<UserOperationResponse> DeleteUser(string userId)
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

        public async Task<UserProfileDataResponse> GetUserByID(string userId)
        {
            try
            {
                return await webApiClient.Get<UserProfileDataResponse>(configuration.Authority, "api/userManagement/user", new { userID = userId }, BuildHeaders);
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
                return await webApiClient.Post<UserOperationResponse>(configuration.Authority, $"api/userManagement/user/{userId}/lock", null, BuildHeaders);
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
